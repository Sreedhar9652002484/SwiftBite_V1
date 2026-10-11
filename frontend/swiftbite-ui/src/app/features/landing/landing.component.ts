import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { OAuthService } from 'angular-oauth2-oidc';
import { AuthService } from '../../core/auth/auth.service';
import { startGoogleLogin } from '../../core/auth/google-login';
import { BrandMarkComponent } from '../../shared/components/brand-mark/brand-mark.component';
import { ThemeToggleComponent } from '../../shared/components/theme-toggle/theme-toggle.component';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [CommonModule, RouterLink, BrandMarkComponent, ThemeToggleComponent],
  templateUrl: './landing.component.html',
})
export class LandingComponent implements OnInit {
  private router  = inject(Router);
  private oauth   = inject(OAuthService);
  private auth    = inject(AuthService);

  signedIn = signal(false);
  starting = signal(false);

  cuisines = [
    { emoji: '🍕', name: 'Pizza' },
    { emoji: '🍔', name: 'Burgers' },
    { emoji: '🍱', name: 'Biryani' },
    { emoji: '🍜', name: 'Chinese' },
    { emoji: '🌮', name: 'Mexican' },
    { emoji: '🥗', name: 'Healthy' },
    { emoji: '🍣', name: 'Japanese' },
    { emoji: '🍦', name: 'Desserts' },
    { emoji: '☕', name: 'Cafe' },
  ];

  steps = [
    { icon: '📍', title: 'Pick your spot', text: 'Choose your delivery address and see restaurants that deliver to you.' },
    { icon: '🍽️', title: 'Choose your food', text: 'Browse menus, filter by veg, and add your favourites in a tap.' },
    { icon: '🛵', title: 'Track it live', text: 'Watch your order move from kitchen to doorstep on a live map.' },
  ];

  features = [
    { icon: '⚡', title: 'Lightning fast', text: 'Short routes and smart dispatch keep your food hot and on time.' },
    { icon: '🛰️', title: 'Live tracking', text: 'Real-time status and rider location, from preparing to delivered.' },
    { icon: '🔒', title: 'Secure payments', text: 'Pay safely with trusted methods. Your details stay protected.' },
    { icon: '🥦', title: 'Made for you', text: 'Veg filters, favourite cuisines and saved addresses for quick reorders.' },
  ];

  ngOnInit(): void {
    if (this.oauth.hasValidAccessToken()) {
      this.signedIn.set(true);
    }
  }

  /** Signed-in users go to their portal; everyone else starts Google sign-in directly. */
  orderNow(): void {
    if (this.signedIn()) {
      this.auth.redirectBasedOnRole();
      return;
    }
    this.starting.set(true);
    void startGoogleLogin();
  }

  goToEmailLogin(): void {
    this.router.navigate(['/auth/login']);
  }
}
