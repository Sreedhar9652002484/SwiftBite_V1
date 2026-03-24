import { Component, OnInit, signal } from '@angular/core';
import { CommonModule }              from '@angular/common';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService }               from '../../../core/auth/auth.service';
import { RestaurantService }         from '../../../core/services/restaurant.service';

@Component({
  selector: 'app-owner-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './owner-layout.component.html',
  styleUrls: ['./owner-layout.component.scss'],
})
export class OwnerLayoutComponent implements OnInit {

  restaurantName = signal('My Restaurant');
  isOpen         = signal(false);
  togglingStatus = signal(false);

  navItems = [
    { label: 'Orders',    icon: '🧾', path: '/owner/dashboard'  },
    { label: 'Menu',      icon: '🍽️', path: '/owner/menu'       },
    { label: 'Settings',  icon: '⚙️',  path: '/owner/settings'   },
    { label: 'Analytics', icon: '📊', path: '/owner/analytics'  },
  ];

  constructor(
    public  auth:    AuthService,
    private restSvc: RestaurantService,
  ) {}

  ngOnInit(): void {
    this.loadRestaurant();
  }

  private getRestaurantId(): string | null {
    const user = this.auth.currentUser();
    // Cover all possible claim name variations from IdentityServer
    return user?.['restaurantId']
        ?? user?.['restaurant_id']
        ?? user?.['RestaurantId']
        ?? null;
  }

  private loadRestaurant(): void {
    const rid = this.getRestaurantId();
    if (!rid) {
      console.warn('SwiftBite: No restaurantId in user claims. Check IdentityServer ProfileService.');
      return;
    }

    // ✅ Uses getById — your existing RestaurantService method
    this.restSvc.getById(rid).subscribe({
      next:  r   => { this.restaurantName.set(r.name); this.isOpen.set(r.isOpen ?? false); },
      error: err => console.error('Could not load restaurant:', err),
    });
  }

  toggleStatus(): void {
    if (this.togglingStatus()) return;
    const rid = this.getRestaurantId();
    if (!rid) return;

    this.togglingStatus.set(true);
    this.restSvc.toggleRestaurantStatus(rid).subscribe({
      next:  res => { this.isOpen.set(res.isOpen); this.togglingStatus.set(false); },
      error: ()  => this.togglingStatus.set(false),
    });
  }

  logout(): void {
    this.auth.logout();
  }
}