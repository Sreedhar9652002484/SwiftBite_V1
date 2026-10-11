import { ApplicationConfig, provideBrowserGlobalErrorListeners, provideZoneChangeDetection } from '@angular/core';
import { provideRouter, withNavigationErrorHandler } from '@angular/router';

import { routes } from './app.routes';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideOAuthClient } from 'angular-oauth2-oidc';
import { jwtInterceptor } from './core/interceptors/jwt.interceptor';
import { errorInterceptor } from './core/interceptors/error.interceptor';
import { responseInterceptor } from './core/interceptors/response.interceptor';

/**
 * After a new deploy, a tab opened on the previous build still references
 * lazy chunks whose hashed files no longer exist, so navigating to a lazy
 * route fails with "Failed to fetch dynamically imported module". Reload once
 * to pick up the new build; the sessionStorage flag prevents a reload loop.
 */
function recoverFromStaleChunk(error: { error?: unknown; url?: string }): void {
  const msg = String((error.error as Error)?.message ?? error.error ?? '');
  if (!/dynamically imported module|Loading chunk|Importing a module script failed/i.test(msg)) {
    return;
  }
  try {
    if (sessionStorage.getItem('chunk_reload') === '1') return;
    sessionStorage.setItem('chunk_reload', '1');
  } catch { /* storage unavailable: reload once anyway */ }
  window.location.assign(error.url ?? window.location.href);
}

export const appConfig: ApplicationConfig = {
  providers: [
      provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes, withNavigationErrorHandler(recoverFromStaleChunk)),
    // No global loading interceptor: each screen owns its own loading feedback
    // (skeletons, inline "Loading X..." text, button state) so it never stacks
    // with a redundant full-screen overlay. LoadingService/app-loading is kept
    // for the rare case a screen has no content to show a skeleton for.
    provideHttpClient(withInterceptors([responseInterceptor, jwtInterceptor,
      errorInterceptor])),
    provideOAuthClient()   // ← registers angular-oauth2-oidc
  ]
};
