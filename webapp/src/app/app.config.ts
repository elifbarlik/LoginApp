import { ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { errorInterceptor } from './core/interceptors/error.interceptor';
import { provideAnimations } from '@angular/platform-browser/animations';

import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    // Order matters: error first, then auth. Response path is reversed → auth handles 401/refresh first, error shows remaining
    provideHttpClient(withInterceptors([errorInterceptor, authInterceptor])),
    provideAnimations()
  ]
};
