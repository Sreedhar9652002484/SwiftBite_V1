import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { OAuthStorage } from 'angular-oauth2-oidc';
import { environment } from '../../../environments/environment';

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const storage = inject(OAuthStorage);
  const token = storage.getItem('access_token');

  if (!token || req.url.includes(environment.authServerUrl)) {
    return next(req);
  }

 // ✅ Add Bearer token for Gateway calls and any other API calls
  if (req.url.includes(environment.apiGatewayUrl) || req.url.startsWith('http')) {
    req = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    });
  }
  return next(req);
};