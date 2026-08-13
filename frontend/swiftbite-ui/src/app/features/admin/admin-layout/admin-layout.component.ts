import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { ThemeToggleComponent } from '../../../shared/components/theme-toggle/theme-toggle.component';

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive, ThemeToggleComponent],
  templateUrl: './admin-layout.component.html',
  styleUrls: ['./admin-layout.component.scss'],
})
export class AdminLayoutComponent {
  constructor(public auth: AuthService, private router: Router) {}

  mobileMenuOpen = signal(false);

  navItems = [
    { label: 'Overview',    icon: '📊', path: '/admin/dashboard'     },
    { label: 'Restaurants', icon: '🍴', path: '/admin/restaurants'   },
    { label: 'Orders',      icon: '🧾', path: '/admin/orders'        },
    { label: 'Analytics',   icon: '📈', path: '/admin/analytics'     },
    { label: 'Applications', icon: '📝', path: '/admin/partner-applications' },
  ];

  logout(): void { this.auth.logout(); }

  toggleMobileMenu(): void { this.mobileMenuOpen.update(v => !v); }
  closeMobileMenu(): void { this.mobileMenuOpen.set(false); }
}