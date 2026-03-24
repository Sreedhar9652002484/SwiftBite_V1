import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { finalize } from 'rxjs';
import { LoadingService }
  from '../services/loading.service';
import { environment } from '../../../environments/environment';

const SKIP_URLS = [
  '/api/notifications/unread-count',
  '/hubs/notifications',
  '/connect/userinfo',
  '/connect/token',        // ✅ Skip auth calls
  '/.well-known',          // ✅ Skip discovery doc
];

export const loadingInterceptor: HttpInterceptorFn =
  (req, next) => {
    const loading = inject(LoadingService);

    // ✅ Skip auth server entirely
    const isAuthCall =
      req.url.includes(environment.authServerUrl);

    const shouldSkip = isAuthCall ||
      SKIP_URLS.some(url => req.url.includes(url));

    if (shouldSkip) return next(req);

    loading.show();
    return next(req).pipe(
      finalize(() => loading.hide())
    );
  };