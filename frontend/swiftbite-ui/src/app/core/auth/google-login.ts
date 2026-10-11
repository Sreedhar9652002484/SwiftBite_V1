import { environment } from '../../../environments/environment';

/**
 * Starts the "Continue with Google" flow: builds the PKCE authorize URL and
 * sends the browser straight to Google via the Auth Server's ExternalLogin
 * endpoint, skipping the Auth Server's own login page. The callback page
 * finishes the code exchange using the values stored here.
 */
export async function startGoogleLogin(): Promise<void> {
  const scope = encodeURIComponent(
    'openid profile email offline_access swiftbite.user swiftbite.order'
  );
  const redirectUri = encodeURIComponent(environment.angularBaseUrl + '/auth/callback');

  const state    = randomString(32);
  const nonce    = randomString(32);
  const verifier = randomString(64);

  sessionStorage.setItem('pkce_verifier', verifier);
  sessionStorage.setItem('oauth_state',   state);
  sessionStorage.setItem('oauth_nonce',   nonce);

  const challenge = await codeChallenge(verifier);

  const authorizeUrl =
    `${environment.authServerUrl}/connect/authorize` +
    `?response_type=code` +
    `&client_id=swiftbite-angular` +
    `&scope=${scope}` +
    `&redirect_uri=${redirectUri}` +
    `&state=${state}` +
    `&nonce=${nonce}` +
    `&code_challenge=${encodeURIComponent(challenge)}` +
    `&code_challenge_method=S256`;

  window.location.href =
    `${environment.authServerUrl}/Account/ExternalLogin` +
    `?provider=Google` +
    `&returnUrl=${encodeURIComponent(authorizeUrl)}`;
}

function randomString(length: number): string {
  const array = new Uint8Array(length);
  crypto.getRandomValues(array);
  return base64Url(array);
}

async function codeChallenge(verifier: string): Promise<string> {
  const digest = await crypto.subtle.digest('SHA-256', new TextEncoder().encode(verifier));
  return base64Url(new Uint8Array(digest));
}

function base64Url(bytes: Uint8Array): string {
  return btoa(String.fromCharCode(...bytes))
    .replace(/\+/g, '-')
    .replace(/\//g, '_')
    .replace(/=/g, '');
}
