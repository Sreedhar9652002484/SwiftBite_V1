import { Injectable, signal, inject } from '@angular/core';
import { OAuthService } from 'angular-oauth2-oidc';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { authConfig } from './auth.config';
import { tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

export interface RegisterRequest {
  firstName:       string;
  lastName:        string;
  email:           string;
  password:        string;
  confirmPassword: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {

  isLoggedIn  = signal<boolean>(false);
  currentUser = signal<any>(null);

  private initComplete = false;
  private initResolvers: (() => void)[] = [];

  private discoveryLoaded = false;
  private authServerUrl   = environment.authServerUrl;
  private apiGatewayUrl   = environment.apiGatewayUrl;

  private oauthService = inject(OAuthService);
  private router       = inject(Router);
  private http         = inject(HttpClient);

  constructor() {
    this.initialize();
  }

  waitForInit(): Promise<void> {
    if (this.initComplete) return Promise.resolve();
    return new Promise(resolve => this.initResolvers.push(resolve));
  }

  private markInitComplete(): void {
    this.initComplete = true;
    this.initResolvers.forEach(r => r());
    this.initResolvers = [];
  }

  // ── App startup ───────────────────────────────────────────
  private async initialize(): Promise<void> {
    this.oauthService.configure(authConfig);

    try {
      await this.oauthService.loadDiscoveryDocument(
        `${this.authServerUrl}/.well-known/openid-configuration`
      );
      this.discoveryLoaded = true;

      // ✅ Check if already logged in
      if (this.oauthService.hasValidAccessToken()) {
        this.isLoggedIn.set(true);
        
        // ✅ Load profile BEFORE marking complete
        await this.loadUserProfile();
        console.log('✅ Session restored');

        // ✅ Only redirect on default routes
        const path = window.location.pathname;
        const isDefaultRoute = path === '/' || path === '/home' || path === '';
        if (isDefaultRoute) {
          this.redirectBasedOnRole();
        }
      }

      // ✅ Setup silent refresh
      this.oauthService.setupAutomaticSilentRefresh();

    } catch (err) {
      console.error('❌ Init error:', err);
    } finally {
      // ✅ Always mark complete
      this.markInitComplete();
    }
  }

  async ensureDiscoveryLoaded(): Promise<void> {
    if (this.discoveryLoaded) return;
    for (let i = 0; i < 50; i++) {
      await new Promise(r => setTimeout(r, 100));
      if (this.discoveryLoaded) return;
    }
    await this.oauthService.loadDiscoveryDocument(
      `${this.authServerUrl}/.well-known/openid-configuration`
    );
    this.discoveryLoaded = true;
  }

  // ── Direct login ──────────────────────────────────────────
  async loginDirect(email: string, password: string): Promise<boolean> {
    try {
      await this.ensureDiscoveryLoaded();
      await this.oauthService.fetchTokenUsingPasswordFlow(email, password);
      this.isLoggedIn.set(true);
      
      // ✅ Load profile after login
      await this.loadUserProfile();
      console.log('✅ Login successful');
      return true;
    } catch (error: any) {
      console.error('❌ Login failed:', error?.message || error);
      return false;
    }
  }

  loginWithRedirect(): void {
    this.oauthService.initCodeFlow();
  }

  register(request: RegisterRequest) {
    return this.http
      .post(`${this.apiGatewayUrl}/api/auth/register`, request)
      .pipe(tap(() => console.log('✅ Registered')));
  }

  logout(): void {
    this.oauthService.logOut(true);
    this.isLoggedIn.set(false);
    this.currentUser.set(null);

    // ✅ Call backend logout
    fetch(`${this.authServerUrl}/Account/SignOut`, {
      method: 'GET',
      credentials: 'include',
      mode: 'no-cors'
    }).finally(() => {
      this.router.navigate(['/auth/login']);
    });
  }

  redirectBasedOnRole(): void {
    if (this.hasRole('Admin'))           this.router.navigate(['/admin/dashboard']);
    else if (this.hasRole('RestaurantAdmin')) this.router.navigate(['/owner/dashboard']);
    else if (this.hasRole('DeliveryPartner')) this.router.navigate(['/delivery/dashboard']);
    else this.router.navigate(['/home']);
  }

  // ── Load user profile from /connect/userinfo ──────────────
  async loadUserProfile(): Promise<void> {
    try {
      // ✅ Use OAuthService.getAccessToken() NOT sessionStorage
      const token = this.oauthService.getAccessToken();
      if (!token) {
        console.warn('⚠️ No token available for profile load');
        return;
      }

      const response = await fetch(
        `${this.authServerUrl}/connect/userinfo`,
        { 
          headers: { Authorization: `Bearer ${token}` },
          credentials: 'include'
        }
      );

      if (response.ok) {
        const profile = await response.json();
        this.currentUser.set(profile);
        console.log('✅ Profile loaded:', profile);
      } else {
        console.error('❌ Profile load failed:', response.status);
      }
    } catch (error) {
      console.error('❌ Profile load error:', error);
    }
  }

  getToken(): string { return this.oauthService.getAccessToken(); }
  getUserClaims(): any { return this.oauthService.getIdentityClaims(); }

  hasRole(role: string): boolean {
    const user = this.currentUser();
    if (user) {
      const roles = user['role'];
      if (Array.isArray(roles)) return roles.includes(role);
      return roles === role;
    }
    const claims = this.getUserClaims();
    if (!claims) return false;
    const roles = claims['role'];
    if (Array.isArray(roles)) return roles.includes(role);
    return roles === role;
  }

  get firstName(): string {
    return this.currentUser()?.['given_name']
        ?? this.currentUser()?.['firstName']
        ?? this.getUserClaims()?.['given_name']
        ?? '';
  }

  get userEmail(): string {
    return this.currentUser()?.['email']
        ?? this.getUserClaims()?.['email']
        ?? '';
  }
}