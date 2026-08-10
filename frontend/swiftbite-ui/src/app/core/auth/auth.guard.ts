import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { OAuthService } from 'angular-oauth2-oidc';
import { AuthService } from './auth.service';

export const authGuard: CanActivateFn = async (route, state) => {
  const oauthService = inject(OAuthService);
  const authService = inject(AuthService);
  const router = inject(Router);

  // ✅ Use OAuthService for token validation (uses localStorage)
  const hasToken = oauthService.hasValidAccessToken();

  if (!hasToken) {
    console.warn('⚠️ No valid token, redirecting to login');
    router.navigate(['/auth/login'], { queryParams: { returnUrl: state.url } });
    return false;
  }

  // ✅ Wait for profile to load
  await authService.waitForInit();

  if (!authService.isLoggedIn()) {
    authService.isLoggedIn.set(true);
  }

  return true;
};

export const roleGuard = (requiredRole: string): CanActivateFn => {
  return async () => {
    const authService = inject(AuthService);
    const router = inject(Router);

    await authService.waitForInit();

    if (authService.hasRole(requiredRole)) {
      return true;
    }

    console.warn(`⚠️ User missing required role: ${requiredRole}`);
    router.navigate(['/unauthorized']);
    return false;
  };
};