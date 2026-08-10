import { Component, inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { CommonModule } from '@angular/common';
import { OAuthService } from 'angular-oauth2-oidc';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './login.component.html'
})
export class LoginComponent {
  private fb           = inject(FormBuilder);
  private authService  = inject(AuthService);
  private router       = inject(Router);
  private route        = inject(ActivatedRoute);
  private oauthService = inject(OAuthService);

  loginForm:     FormGroup;
  isLoading      = signal(false);
  errorMessage   = signal('');
  successMessage = signal('');
  showPassword   = signal(false);

  constructor() {
    this.loginForm = this.fb.group({
      email:    ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8)]]
    });
  }

  async onSubmit(): Promise<void> {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set('');
    this.successMessage.set('');

    const { email, password } = this.loginForm.value;
    const success = await this.authService.loginDirect(email, password);

    if (success) {
      this.successMessage.set('Login successful! Redirecting...');
      // ✅ Use redirectBasedOnRole — not hardcoded /dashboard
      this.authService.redirectBasedOnRole();
    } else {
      this.errorMessage.set('Invalid email or password. Please try again.');
    }

    this.isLoading.set(false);
  }

  loginWithOAuth(): void {
    this.authService.loginWithRedirect();
  }


loginWithGoogle(): void {
  // ✅ Go DIRECTLY to Google via Auth Server
  // Bypasses Auth Server login page completely
  // returnUrl carries the authorize URL so OAuth2 completes
  
  const scope = encodeURIComponent(
    'openid profile email offline_access swiftbite.user swiftbite.order'
  );
  const redirectUri = encodeURIComponent(
    environment.angularBaseUrl + '/auth/callback'
  );

  // Build authorize URL manually WITH our own state
  const state    = this.generateRandom(32);
  const nonce    = this.generateRandom(32);
  const verifier = this.generateRandom(64);

  // Store PKCE verifier in sessionStorage BEFORE navigation
  sessionStorage.setItem('pkce_verifier', verifier);
  sessionStorage.setItem('oauth_state',   state);
  sessionStorage.setItem('oauth_nonce',   nonce);

  // Generate code challenge
  this.generateCodeChallenge(verifier).then(challenge => {
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

    // Go DIRECTLY to Google — skips Auth Server login page
    window.location.href =
      `${environment.authServerUrl}/Account/ExternalLogin` +
      `?provider=Google` +
      `&returnUrl=${encodeURIComponent(authorizeUrl)}`;
  });
}

private generateRandom(length: number): string {
  const array = new Uint8Array(length);
  crypto.getRandomValues(array);
  return btoa(String.fromCharCode(...array))
    .replace(/\+/g, '-')
    .replace(/\//g, '_')
    .replace(/=/g, '');
}

private async generateCodeChallenge(verifier: string): Promise<string> {
  const encoder = new TextEncoder();
  const data    = encoder.encode(verifier);
  const digest  = await crypto.subtle.digest('SHA-256', data);
  return btoa(String.fromCharCode(...new Uint8Array(digest)))
    .replace(/\+/g, '-')
    .replace(/\//g, '_')
    .replace(/=/g, '');
}

  togglePassword(): void {
    this.showPassword.update(v => !v);
  }

  get email()    { return this.loginForm.get('email'); }
  get password() { return this.loginForm.get('password'); }
}