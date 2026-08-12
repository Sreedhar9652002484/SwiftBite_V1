import { ApplicationConfig, provideBrowserGlobalErrorListeners, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideOAuthClient } from 'angular-oauth2-oidc';
import { jwtInterceptor } from './core/interceptors/jwt.interceptor';
import { errorInterceptor } from './core/interceptors/error.interceptor';
import { responseInterceptor } from './core/interceptors/response.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
      provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    // No global loading interceptor: each screen owns its own loading feedback
    // (skeletons, inline "Loading X..." text, button state) so it never stacks
    // with a redundant full-screen overlay. LoadingService/app-loading is kept
    // for the rare case a screen has no content to show a skeleton for.
    provideHttpClient(withInterceptors([responseInterceptor, jwtInterceptor,
      errorInterceptor])),
    provideOAuthClient()   // ← registers angular-oauth2-oidc
  ]
};
