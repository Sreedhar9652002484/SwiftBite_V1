import { HttpInterceptorFn, HttpResponse } from '@angular/common/http';
import { tap } from 'rxjs';

/**
 * ✅ NEW: Response logger for debugging
 * Logs successful responses with standardized format
 */
export const responseInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    tap((event: any) => {
      // Only process HTTP responses, not other events
      if (event instanceof HttpResponse) {
        const body = event.body as {
          success?: boolean;
          message?: string;
          data?: any;
          statusCode?: number;
          traceId?: string;
        };

        // ✅ Log standardized responses
        if (body && typeof body === 'object' && 'success' in body) {
          if (body.success) {
            console.log('✅ Response Success:', {
              endpoint: req.url,
              status: event.status,
              message: body.message,
              hasData: !!body.data,
              traceId: body.traceId
            });
          }
        }
      }
    })
  );
};