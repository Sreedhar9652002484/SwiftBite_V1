import { Component, inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { CommonModule } from '@angular/common';
import { OAuthService } from 'angular-oauth2-oidc';
import { startGoogleLogin } from '../../../core/auth/google-login';
import { BrandMarkComponent } from '../../../shared/components/brand-mark/brand-mark.component';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, BrandMarkComponent],
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
    void startGoogleLogin();
  }

  togglePassword(): void {
    this.showPassword.update(v => !v);
  }

  get email()    { return this.loginForm.get('email'); }
  get password() { return this.loginForm.get('password'); }
}