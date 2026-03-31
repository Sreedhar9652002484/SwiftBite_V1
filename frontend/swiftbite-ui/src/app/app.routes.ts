import { Routes } from '@angular/router';
import { authGuard }  from './core/auth/auth.guard';
import { ownerGuard } from './core/auth/owner.guard';   // ← add this import
import { adminGuard } from './core/auth/admin.guard';
import { deliveryGuard } from './core/auth/delivery.gaurd';

export const routes: Routes = [
  { path: '', redirectTo: 'home', pathMatch: 'full' },

  // ── Auth ──────────────────────────────────────────────
  {
    path: 'auth',
    loadChildren: () =>
      import('./features/auth/auth.routes').then(m => m.AUTH_ROUTES),
  },

  // ── Customer ──────────────────────────────────────────
  {
    path: 'home',
    loadComponent: () =>
      import('./features/customer/home/home.component')
        .then(m => m.HomeComponent),
    canActivate: [authGuard],
  },
  {
    path: 'restaurant/:id',
    loadComponent: () =>
      import('./features/customer/restaurant-detail/restaurant-detail.component')
        .then(m => m.RestaurantDetailComponent),
    canActivate: [authGuard],
  },
  {
    path: 'checkout',
    loadComponent: () =>
      import('./features/customer/checkout/checkout.component')
        .then(m => m.CheckoutComponent),
    canActivate: [authGuard],
  },
  {
    path: 'orders/:id/track',
    loadComponent: () =>
      import('./features/customer/order-tracking/order-tracking.component')
        .then(m => m.OrderTrackingComponent),
    canActivate: [authGuard],
  },
  {
    path: 'orders',
    loadComponent: () =>
      import('./features/customer/order-history/order-history.component')
        .then(m => m.OrderHistoryComponent),
    canActivate: [authGuard],
  },
  {
    path: 'profile',
    loadComponent: () =>
      import('./features/customer/profile/profile.component')
        .then(m => m.ProfileComponent),
    canActivate: [authGuard],
  },

  // ── Admin ─────────────────────────────────────────────
  {
    path: 'admin',
    loadComponent: () =>
      import('./features/admin/admin-layout/admin-layout.component')
        .then(m => m.AdminLayoutComponent),
    canActivate: [authGuard, adminGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./features/admin/dashboard/admin-dashboard.component')
            .then(m => m.AdminDashboardComponent),
      },
      {
        path: 'restaurants',
        loadComponent: () =>
          import('./features/admin/restaurants/admin-restaurants.component')
            .then(m => m.AdminRestaurantsComponent),
      },
      {
        path: 'orders',
        loadComponent: () =>
          import('./features/admin/orders/admin-orders.component')
            .then(m => m.AdminOrdersComponent),
      },
      {
        path: 'analytics',
        loadComponent: () =>
          import('./features/admin/analytics/admin-analytics.component')
            .then(m => m.AdminAnalyticsComponent),
      },
    ],
  },

 // ── Restaurant Owner ───────────────────────────────────
  // Uses /owner prefix to avoid conflict with /restaurant/:id
  {
    path: 'owner',
    loadComponent: () =>
      import('./features/restaurents/owner-layout/Owner layout.component')
        .then(m => m.OwnerLayoutComponent),
    canActivate: [authGuard, ownerGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./features/restaurents/dashboard/restaurant-dashboard.component')
            .then(m => m.RestaurantDashboardComponent),
      },
      {
        path: 'menu',
        loadComponent: () =>
          import('./features/restaurents/menu-manager/menu-manager.component')
            .then(m => m.MenuManagerComponent),
      },
      {
        path: 'settings',
        loadComponent: () =>
          import('./features/restaurents/restaurant-settings/restaurant-settings.component')
            .then(m => m.RestaurantSettingsComponent),
      },
      {
        path: 'analytics',
        loadComponent: () =>
          import('./features/restaurents/analytics/analytics.component')
            .then(m => m.AnalyticsComponent),
      },
    ],
  },
   // ── Delivery Partner ───────────────────────────────────
  {
    path: 'delivery',
    loadComponent: () =>
      import('./features/delivery/layout/delivery-layout.component')
        .then(m => m.DeliveryLayoutComponent),
    canActivate: [authGuard, deliveryGuard],
    children: [
      { path: '', redirectTo: 'jobs', pathMatch: 'full' },
      { path: 'jobs',     loadComponent: () => import('./features/delivery/jobs/delivery-jobs.component').then(m => m.DeliveryJobsComponent) },
      { path: 'active',   loadComponent: () => import('./features/delivery/jobs/active/delivery-active.component').then(m => m.DeliveryActiveComponent) },
      { path: 'earnings', loadComponent: () => import('./features/delivery/earnings/delivery-earnings.component').then(m => m.EarningsComponent) },
      { path: 'profile',  loadComponent: () => import('./features/delivery/profile/delivery-profile.component').then(m => m.DeliveryProfileComponent) },
    ],
  },
 
  { path: '**', redirectTo: 'home' },
];