import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { OAuthService } from 'angular-oauth2-oidc';
import { AuthService } from '../../../core/auth/auth.service';
import { environment } from '../../../../environments/environment';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { BrandMarkComponent } from '../../../shared/components/brand-mark/brand-mark.component';

@Component({
  selector: 'app-callback',
  standalone: true,
  imports: [CommonModule, BrandMarkComponent],
  template: `
    <div class="min-h-screen sb-auth-bg flex flex-col items-center justify-center gap-5 p-4">
      <div class="sb-logo-tile sb-float"><app-brand-mark /></div>
      <p class="text-ink font-extrabold text-xl tracking-tight">Signing you in…</p>
      <p class="text-muted text-sm -mt-3">Just a moment while we get things ready</p>
      <div class="w-9 h-9 border-4 border-primary-soft border-t-primary rounded-full animate-spin"></div>
    </div>
  `
})
export class CallbackComponent implements OnInit {
  private oauthService = inject(OAuthService);
  private authService  = inject(AuthService);
  private router       = inject(Router);
  private http         = inject(HttpClient);

  async ngOnInit(): Promise<void> {
    try {
      await this.oauthService.loadDiscoveryDocument(
        `${environment.authServerUrl}/.well-known/openid-configuration`
      );

      // Get URL params
      const params   = new URLSearchParams(window.location.search);
      const code     = params.get('code');
      const state    = params.get('state');
      const error    = params.get('error');

      if (error) {
        console.error('OAuth error:', error);
        this.router.navigate(['/auth/login']);
        return;
      }

      if (!code) {
        this.router.navigate(['/auth/login']);
        return;
      }

      // ✅ Check if this is our manual Google flow
      const savedState    = sessionStorage.getItem('oauth_state');
      const savedVerifier = sessionStorage.getItem('pkce_verifier');

      if (savedState && savedVerifier && state === savedState) {
        // Manual Google flow — exchange code manually
        console.log('✅ Manual Google flow — exchanging code');
        await this.exchangeCodeManually(code, savedVerifier);
      } else {
        // Standard OAuthService flow
        console.log('✅ Standard OAuth flow');
        await this.oauthService.tryLogin();

      // ✅ Only ONE tryLogin call — no conflict with AuthService
      await this.oauthService.tryLogin({
        disableOAuth2StateCheck: false,
        onTokenReceived: () => console.log('✅ Token received')
      });

        if (this.oauthService.hasValidAccessToken()) {
          this.authService.isLoggedIn.set(true);
          await this.authService.loadUserProfile();
          this.authService.redirectBasedOnRole();
        } else {
          this.router.navigate(['/auth/login']);
        }
      }
    } catch (err) {
      console.error('Callback error:', err);
      this.router.navigate(['/auth/login']);
    }
  }

  private async exchangeCodeManually(
    code: string,
    verifier: string
  ): Promise<void> {
    try {
      const body = new URLSearchParams({
        grant_type:    'authorization_code',
        code:          code,
        redirect_uri:  environment.angularBaseUrl + '/auth/callback',
        client_id:     'swiftbite-angular',
        code_verifier: verifier
      });

      // Clean up sessionStorage
      sessionStorage.removeItem('pkce_verifier');
      sessionStorage.removeItem('oauth_state');
      sessionStorage.removeItem('oauth_nonce');

      const response = await fetch(
        `${environment.authServerUrl}/connect/token`,
        {
          method: 'POST',
          headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
          body: body.toString()
        }
      );

      if (!response.ok) {
        const err = await response.text();
        console.error('Token exchange failed:', err);
        this.router.navigate(['/auth/login']);
        return;
      }

      const tokens = await response.json();
      console.log('✅ Tokens received!');

      // ✅ Store tokens so OAuthService can use them
      sessionStorage.setItem('access_token',              tokens.access_token);
      sessionStorage.setItem('id_token',                  tokens.id_token ?? '');
      sessionStorage.setItem('refresh_token',             tokens.refresh_token ?? '');
      sessionStorage.setItem('access_token_stored_at',    Date.now().toString());
      sessionStorage.setItem('expires_at',
        (Date.now() + (tokens.expires_in * 1000)).toString());

      this.authService.isLoggedIn.set(true);
      await this.authService.loadUserProfile();
      this.authService.redirectBasedOnRole();

    } catch (err) {
      console.error('Manual exchange error:', err);
      this.router.navigate(['/auth/login']);
    }
  }
}