import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  const accessToken = auth.getAccessToken();
  const cloned = accessToken
    ? req.clone({ setHeaders: { Authorization: `Bearer ${accessToken}` } })
    : req;

  return next(cloned).pipe(
    catchError((err: HttpErrorResponse) => {
      // 401: try refresh flow for non-auth endpoints only
      const isAuthEndpoint = /\/auth\/(login|register|google|refresh)$/i.test(req.url);
      if (err.status === 401 && !isAuthEndpoint) {
        return auth.refresh().pipe(
          switchMap(() => {
            const newToken = auth.getAccessToken();
            const retried = newToken
              ? req.clone({ setHeaders: { Authorization: `Bearer ${newToken}` } })
              : req;
            return next(retried);
          }),
          catchError(innerErr => {
            // On refresh failure (e.g. 400/401), clear local tokens and redirect
            auth.signOutLocal();
            router.navigateByUrl('/login');
            return throwError(() => innerErr);
          })
        );
      }
      // If refresh endpoint itself returns 400 (bad payload), ensure logout path
      if (req.url.includes('/auth/refresh') && (err.status === 400 || err.status === 401)) {
        try { window.alert('Session expired or invalid. Please sign in again.'); } catch {}
        auth.signOutLocal();
        router.navigateByUrl('/login');
      }
      return throwError(() => err);
    })
  );
};


