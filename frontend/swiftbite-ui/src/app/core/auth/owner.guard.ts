import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../auth/auth.service';

export const ownerGuard: CanActivateFn = async () => {
  const auth   = inject(AuthService);
  const router = inject(Router);

  // ✅ Wait for profile to be loaded — role is only available after this
  await auth.waitForInit();

  if (!auth.isLoggedIn()) {
    router.navigate(['/auth/login']);
    return false;
  }

  if (auth.hasRole('RestaurantAdmin') || auth.hasRole('Admin')) {
    return true;
  }

  router.navigate(['/home']);
  return false;
};