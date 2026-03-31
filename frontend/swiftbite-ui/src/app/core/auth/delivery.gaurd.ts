// ── delivery.guard.ts ─────────────────────────────────────────
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../auth/auth.service';

export const deliveryGuard: CanActivateFn = async () => {
  const auth   = inject(AuthService);
  const router = inject(Router);

  await auth.waitForInit();

  if (!auth.isLoggedIn()) {
    router.navigate(['/auth/login']);
    return false;
  }

  if (auth.hasRole('DeliveryPartner')) return true;

  router.navigate(['/home']);
  return false;
};