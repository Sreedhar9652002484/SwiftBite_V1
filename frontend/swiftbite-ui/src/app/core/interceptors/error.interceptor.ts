import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject, Injector, runInInjectionContext } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError, retry, timer } from 'rxjs';
import { ToastService } from '../services/toast.service';
import { LoadingService } from '../services/loading.service';

/**
 * ✅ UPDATED: Handles new ApiResponse<T> format from backend
 * Extracts error details, validation errors, and trace IDs
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const toast = inject(ToastService);
  const loading = inject(LoadingService);
  const router = inject(Router);
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

      // ✅ NEW: Extract from standardized response format
      const errorResponse = error.error as {
        success?: boolean;
        message?: string;
        errorCode?: string;
        title?: string;
        statusCode?: number;
        errors?: Record<string, string[]>;
        details?: string;
        traceId?: string;
      };

      // ✅ Helper: Get main error message
      const getErrorMessage = (): string => {
        if (errorResponse?.message) {
          return errorResponse.message;
        }
        if (errorResponse?.details) {
          return errorResponse.details;
        }
        if (error.message) {
          return error.message;
        }
        return 'An unknown error occurred';
      };

      // ✅ Helper: Format validation errors
      const getValidationErrors = (): string => {
        if (!errorResponse?.errors || Object.keys(errorResponse.errors).length === 0) {
          return '';
        }

        const errorMessages = Object.entries(errorResponse.errors)
          .map(([field, messages]) => {
            const fieldName = field.charAt(0).toUpperCase() + field.slice(1);
            return `• ${fieldName}: ${messages.join(', ')}`;
          })
          .join('\n');

        return errorMessages;
      };

      // ✅ Helper: Log error for debugging
      const logErrorForDebugging = (): void => {
        console.error('🔴 API Error:', {
          status: error.status,
          errorCode: errorResponse?.errorCode,
          message: errorResponse?.message,
          traceId: errorResponse?.traceId,
          timestamp: new Date().toISOString()
        });
        
        // ✅ Log trace ID for backend debugging
        if (errorResponse?.traceId) {
          console.error(`📊 Trace ID for server logs: ${errorResponse.traceId}`);
        }
      };

      logErrorForDebugging();

      switch (error.status) {
        case 0:
          toast.error(
            '🔌 Cannot connect to server. ' +
            'Please try again.');
          break;

        case 400:
          // ✅ NEW: Check for validation errors first
          const validationErrors = getValidationErrors();
          if (validationErrors) {
            const fullMessage = `❌ Validation Error:\n${validationErrors}`;
            toast.error(fullMessage);
          } else {
            const msg400 = getErrorMessage();
            toast.error(`❌ ${msg400}`);
          }
          break;

        case 401:
          // ✅ Session expired - redirect to login
          runInInjectionContext(injector, () => {
            toast.warning('🔐 Session expired. Please login again.');
            sessionStorage.clear();
            localStorage.clear();
            router.navigate(['/auth/login']);
          });
          break;

        case 403:
          // ✅ Access forbidden
          toast.error(`🚫 ${getErrorMessage()}`);
          break;

        case 404:
          // ✅ Silence 404s for background calls like profile check
          if (!req.url.includes('/api/users/profile')) {
            toast.error(`🔍 ${getErrorMessage()}`);
          }
          break;

        case 409:
          // ✅ Conflict error
          const msg409 = getErrorMessage();
          toast.error(`⚠️ ${msg409}`);
          break;

        case 422:
          // ✅ NEW: Business rule violation
          const msg422 = getErrorMessage();
          toast.error(`📋 ${msg422}`);
          break;

        case 429:
          // ✅ Rate limiting
          toast.warning('⏳ Too many requests. Please wait a moment.');
          break;

        case 500:
          // ✅ NEW: Include trace ID in error message
          const traceId = errorResponse?.traceId
            ? ` [Trace: ${errorResponse.traceId}]`
            : '';
          toast.error(`🔥 Server error. Please try again.${traceId}`);
          break;

        case 503:
          // ✅ Service unavailable
          toast.error('🛠️ Service unavailable. Please try again later.');
          break;

        default:
          if (error.status >= 500) {
            toast.error(`🔥 ${getErrorMessage()}`);
          } else if (error.status >= 400) {
            toast.error(`❌ ${getErrorMessage()}`);
          } else {
            toast.error('⚠️ An unexpected error occurred.');
          }
      }

      return throwError(() => error);
    })
  );
};