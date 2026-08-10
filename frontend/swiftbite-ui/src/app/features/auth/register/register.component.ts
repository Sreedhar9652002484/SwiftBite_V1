import { Component, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, AbstractControl, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-register',
   standalone: true,                              // ← MUST have this
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './register.component.html'
})
export class RegisterComponent {
  registerForm: FormGroup;
  isLoading     = signal(false);
  errorMessage  = signal('');
  successMessage = signal('');
  showPassword   = signal(false);

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.registerForm = this.fb.group({
      firstName:       ['', [Validators.required, Validators.maxLength(50)]],
      lastName:        ['', [Validators.required, Validators.maxLength(50)]],
      email:           ['', [Validators.required, Validators.email]],
      password:        ['', [
        Validators.required,
        Validators.minLength(8),
        Validators.pattern(/(?=.*[A-Z])(?=.*[0-9])/)
      ]],
      confirmPassword: ['', Validators.required]
    }, { validators: this.passwordMatchValidator });
  }

  passwordMatchValidator(group: AbstractControl) {
    const pass    = group.get('password')?.value;
    const confirm = group.get('confirmPassword')?.value;
    return pass === confirm ? null : { passwordMismatch: true };
  }

  togglePassword() {
  this.showPassword.update(v => !v);
}
  onSubmit(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set('');

    this.authService.register(this.registerForm.value).subscribe({
      next: () => {
        this.successMessage.set('Account created! Redirecting to login...');
        setTimeout(() => this.router.navigate(['/auth/login']), 2000);
      },
      error: (err) => {
        const errors = err.error?.errors;
        this.errorMessage.set(
          Array.isArray(errors)
            ? errors.join(', ')
            : 'Registration failed. Please try again.'
        );
        this.isLoading.set(false);
      },
      complete: () => this.isLoading.set(false)
    });
  }

  get f() { return this.registerForm.controls; }
}