import { AuthConfig } from 'angular-oauth2-oidc';
import { environment } from '../../../environments/environment';

export const authConfig: AuthConfig = {
  issuer:        environment.authServerUrl,
  clientId:      'swiftbite-angular',
  scope:         'openid profile email offline_access swiftbite.user swiftbite.order',
  responseType:  'code',
  redirectUri:           environment.angularBaseUrl + '/auth/callback',
  postLogoutRedirectUri: environment.angularBaseUrl + '/auth/login',
  requireHttps:                      false,
  skipIssuerCheck:                   true,
  strictDiscoveryDocumentValidation: false,
  sessionChecksEnabled:              false,
  showDebugInformation:              !environment.production,
  oidc:             true,
  disablePKCE:      false,
  useHttpBasicAuth: false,
  logoutUrl: environment.angularBaseUrl + '/auth/login',
};