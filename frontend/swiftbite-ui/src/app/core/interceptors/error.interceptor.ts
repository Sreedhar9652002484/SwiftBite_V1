import { HttpInterceptorFn,
  HttpErrorResponse } from '@angular/common/http';
import { inject, Injector, runInInjectionContext }
  from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError, retry, timer }
  from 'rxjs';
import { ToastService }
  from '../services/toast.service';
import { LoadingService }
  from '../services/loading.service';

export const errorInterceptor: HttpInterceptorFn =
  (req, next) => {
    const toast    = inject(ToastService);
    const loading  = inject(LoadingService);
    const router   = inject(Router);
    const injector = inject(Injector);

    return next(req).pipe(
      retry({
        count: 2,
        delay: (error, attempt) => {
          if (error instanceof HttpErrorResponse
              && error.status >= 400
              && error.status < 500) {
            return throwError(() => error);
          }
          return timer(attempt * 1000);
        }
      }),

      catchError((error: HttpErrorResponse) => {
        loading.forceHide();

        if (!navigator.onLine) {
          toast.error(
            '📡 No internet connection. ' +
            'Check your network and retry.');
          return throwError(() => error);
        }

        switch (error.status) {
          case 0:
            toast.error(
              '🔌 Cannot connect to server. ' +
              'Please try again.');
            break;

          case 400:
            const msg400 = error.error?.message
              || error.error?.title
              || 'Invalid request. Check your input.';
            toast.error(`❌ ${msg400}`);
            break;

          case 401:
            // ✅ Use runInInjectionContext to avoid
            //    circular dependency!
            runInInjectionContext(injector, () => {
              toast.warning(
                '🔐 Session expired. Login again.');
              // ✅ Clear session directly —
              //    no AuthService injection!
              sessionStorage.clear();
              localStorage.clear();
              router.navigate(['/auth/login']);
            });
            break;

          case 403:
            toast.error(
              '🚫 You don\'t have permission.');
            router.navigate(['/unauthorized']);
            break;

          case 404:
            // ✅ Silence 404s for background calls
            if (!req.url.includes('/api/users/profile'))
              toast.error('🔍 Resource not found.');
            break;

          case 409:
            const msg409 = error.error?.message
              || 'Conflict — already exists.';
            toast.error(`⚠️ ${msg409}`);
            break;

          case 429:
            toast.warning(
              '⏳ Too many requests. Wait a moment.');
            break;

          case 500:
            toast.error(
              '🔥 Server error. Try again.');
            break;

          case 503:
            toast.error(
              '🛠️ Service unavailable. Try later.');
            break;

          default:
            if (error.status >= 500)
              toast.error('🔥 Something went wrong.');
        }

        return throwError(() => error);
      })
    );
  };