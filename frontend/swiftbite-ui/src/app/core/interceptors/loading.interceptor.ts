import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { finalize } from 'rxjs';
import { LoadingService } from '../services/loading.service';
import { environment } from '../../../environments/environment';

const SKIP_URLS = [
  '/api/notifications/unread-count',
  '/hubs/notifications',
  '/connect/userinfo',
  '/connect/token',
  '/.well-known',
  '/Account/Login',
  '/Account/ExternalLogin',
  '/auth/login',
  '/auth/register'
];

export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  const loading = inject(LoadingService);

  const isAuthCall = req.url.includes(environment.authServerUrl);
  const shouldSkip = isAuthCall || 
    SKIP_URLS.some(url => req.url.includes(url));

  if (shouldSkip) return next(req);

  loading.show();
  return next(req).pipe(
    finalize(() => loading.hide())
  );
};