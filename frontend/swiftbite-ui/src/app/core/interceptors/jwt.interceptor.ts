import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { OAuthService } from 'angular-oauth2-oidc';
import { environment } from '../../../environments/environment';

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const oauthService = inject(OAuthService);
  
  // ✅ Get token from OAuthService (uses localStorage)
  const token = oauthService.getAccessToken();

  // 🔓 Public endpoints that DON'T need auth
  const publicEndpoints = [
    '/auth/login',
    '/auth/register',
    '/auth/callback',
    '/connect/authorize',
    '/connect/token',
    '/.well-known/openid-configuration',
    '/Account/Login',
    '/Account/ExternalLogin',
    '/api/auth/login',
    '/api/auth/register'
  ];

  const isPublic = publicEndpoints.some(ep => req.url.includes(ep));

  // ✅ Add token to ALL authenticated requests
  if (token && !isPublic) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
    console.log('🔐 Token added to:', req.url);
  }

  return next(req);
};