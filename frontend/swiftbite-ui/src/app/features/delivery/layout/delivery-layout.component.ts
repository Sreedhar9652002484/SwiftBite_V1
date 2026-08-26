import { Component, OnInit, OnDestroy, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { DeliveryService } from '../../../core/services/delivery.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ThemeToggleComponent } from '../../../shared/components/theme-toggle/theme-toggle.component';

@Component({
  selector: 'app-delivery-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive, ThemeToggleComponent],
  templateUrl: './delivery-layout.component.html',
  styleUrls: ['./delivery-layout.component.scss'],
})
export class DeliveryLayoutComponent implements OnInit, OnDestroy {

  auth        = inject(AuthService);
  deliverySvc = inject(DeliveryService);
  private notifSvc = inject(NotificationService);

  isAvailable    = signal(false);
  partnerName    = signal('Partner');
  togglingStatus = signal(false);
  mobileMenuOpen = signal(false);

  navItems = [
      { label: 'Dashboard', icon: '🏠', path: '/delivery/dashboard' }, // ← ADD
    { label: 'Jobs',     icon: '📦', path: '/delivery/jobs'     },
    { label: 'Active',   icon: '🚴', path: '/delivery/active'   },
    { label: 'Earnings', icon: '₹',  path: '/delivery/earnings' },
    { label: 'Profile',  icon: '👤', path: '/delivery/profile'  },
  ];

  ngOnInit(): void {
    this.deliverySvc.getProfile().subscribe({
      next: p => {
        this.isAvailable.set(p.isAvailable);
        this.partnerName.set(p.firstName);
      },
      error: () => {} // not yet registered — handled by profile page
    });

    this.notifSvc.connectSignalR();
  }

  ngOnDestroy(): void {
    this.notifSvc.disconnectSignalR();
  }

  toggleAvailability(): void {
    if (this.togglingStatus()) return;
    this.togglingStatus.set(true);
    this.deliverySvc.updateAvailability(!this.isAvailable()).subscribe({
      next:  p => { this.isAvailable.set(p.isAvailable); this.togglingStatus.set(false); },
      error: () => this.togglingStatus.set(false),
    });
  }

  logout(): void { this.auth.logout(); }

  toggleMobileMenu(): void { this.mobileMenuOpen.update(v => !v); }
  closeMobileMenu(): void { this.mobileMenuOpen.set(false); }
}