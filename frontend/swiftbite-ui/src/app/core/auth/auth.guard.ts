import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { OAuthService } from 'angular-oauth2-oidc';
import { AuthService } from './auth.service';

export const authGuard: CanActivateFn = async (route, state) => {
  const oauthService = inject(OAuthService);
  const authService  = inject(AuthService);
  const router       = inject(Router);

  // ✅ Your original token check — kept exactly as-is
  const token     = sessionStorage.getItem('access_token');
  const expiresAt = sessionStorage.getItem('expires_at');
  const isValid   = token && expiresAt && Date.now() < parseInt(expiresAt);
  const hasToken  = isValid || oauthService.hasValidAccessToken();

  if (!hasToken) {
    router.navigate(['/auth/login'], { queryParams: { returnUrl: state.url } });
    return false;
  }

  // ✅ KEY FIX: wait for loadUserProfile() to complete
  // On refresh, token exists but currentUser is still null
  // Guards were running before profile loaded → hasRole() always false
  await authService.waitForInit();

  if (!authService.isLoggedIn()) {
    authService.isLoggedIn.set(true);
  }

  return true;
};

export const roleGuard = (requiredRole: string): CanActivateFn => {
  return async () => {
    const authService = inject(AuthService);
    const router      = inject(Router);

    // waitForInit() resolves instantly if already done — no extra delay
    await authService.waitForInit();

    if (authService.hasRole(requiredRole)) return true;
    router.navigate(['/unauthorized']);
    return false;
  };
};