import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, map, tap, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';

type AuthResponse = {
  accessToken: string;
  refreshToken: string;
  role: string;
};

type LoginRequest = { email: string; password: string };
type RegisterRequest = { email: string; username: string; password: string; role?: string | null };
type GoogleLoginRequest = { idToken: string };
type RefreshRequest = { accessToken: string; refreshToken: string };
type LogoutRequest = { refreshToken: string };

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly baseUrl = environment.apiUrl;
  private readonly accessTokenKey = 'accessToken';
  private readonly refreshTokenKey = 'refreshToken';
  private readonly roleKey = 'role';

  private readonly loggedIn$ = new BehaviorSubject<boolean>(this.hasValidTokens());

  constructor(private http: HttpClient) {}

  register(request: RegisterRequest): Observable<void> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/auth/register`, request).pipe(
      tap((res) => this.storeTokens(res)),
      map(() => void 0)
    );
  }

  login(request: LoginRequest): Observable<void> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/auth/login`, request).pipe(
      tap((res) => this.storeTokens(res)),
      map(() => void 0)
    );
  }

  loginWithGoogle(idToken: string): Observable<void> {
    const body: GoogleLoginRequest = { idToken };
    return this.http.post<AuthResponse>(`${this.baseUrl}/auth/google`, body).pipe(
      tap((res) => this.storeTokens(res)),
      map(() => void 0)
    );
  }

  refresh(): Observable<void> {
    const access = this.getAccessToken();
    const refresh = this.getRefreshToken();
    if (!access || !refresh) {
      return throwError(() => new Error('No tokens available for refresh'));
    }
    const payload: RefreshRequest = {
      accessToken: access,
      refreshToken: refresh
    };
    return this.http.post<AuthResponse>(`${this.baseUrl}/auth/refresh`, payload).pipe(
      tap((res) => this.storeTokens(res)),
      map(() => void 0)
    );
  }

  logout(): Observable<void> {
    const body: LogoutRequest = { refreshToken: this.getRefreshToken() ?? '' };
    return this.http.post(`${this.baseUrl}/auth/logout`, body).pipe(
      tap(() => this.clearTokens()),
      map(() => void 0)
    );
  }

  isAuthenticated(): boolean {
    return this.loggedIn$.value;
  }

  observeAuth(): Observable<boolean> {
    return this.loggedIn$.asObservable();
  }

  getAccessToken(): string | null {
    return localStorage.getItem(this.accessTokenKey);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.refreshTokenKey);
  }

  getRole(): string | null {
    return localStorage.getItem(this.roleKey);
  }

  // Expose a safe way to clear local tokens without calling API
  signOutLocal(): void {
    this.clearTokens();
  }

  private storeTokens(res: AuthResponse) {
    localStorage.setItem(this.accessTokenKey, res.accessToken);
    localStorage.setItem(this.refreshTokenKey, res.refreshToken);
    localStorage.setItem(this.roleKey, res.role);
    this.loggedIn$.next(true);
  }

  private clearTokens() {
    localStorage.removeItem(this.accessTokenKey);
    localStorage.removeItem(this.refreshTokenKey);
    localStorage.removeItem(this.roleKey);
    this.loggedIn$.next(false);
  }

  private hasValidTokens(): boolean {
    return !!this.getAccessToken() && !!this.getRefreshToken();
  }
}


