import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './admin-layout.component.html',
  styleUrls: ['./admin-layout.component.scss'],
})
export class AdminLayoutComponent {
  constructor(public auth: AuthService, private router: Router) {}

  navItems = [
    { label: 'Overview',    icon: '📊', path: '/admin/dashboard'     },
    { label: 'Restaurants', icon: '🍴', path: '/admin/restaurants'   },
    { label: 'Orders',      icon: '🧾', path: '/admin/orders'        },
    { label: 'Analytics',   icon: '📈', path: '/admin/analytics'     },
  ];

  logout(): void { this.auth.logout(); }
}