import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../auth/auth.service';

export const ownerGuard: CanActivateFn = async () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  // ✅ Wait for profile to load
  await auth.waitForInit();

  if (!auth.isLoggedIn()) {
    console.warn('⚠️ User not logged in');
    router.navigate(['/auth/login']);
    return false;
  }

  if (auth.hasRole('RestaurantAdmin') || auth.hasRole('Admin')) {
    return true;
  }

  console.warn('⚠️ User is not a RestaurantAdmin');
  router.navigate(['/home']);
  return false;
};