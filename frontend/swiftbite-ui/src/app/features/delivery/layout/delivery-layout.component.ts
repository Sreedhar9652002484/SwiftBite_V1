import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { DeliveryService } from '../../../core/services/delivery.service';

@Component({
  selector: 'app-delivery-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './delivery-layout.component.html',
  styleUrls: ['./delivery-layout.component.scss'],
})
export class DeliveryLayoutComponent implements OnInit {

  auth        = inject(AuthService);
  deliverySvc = inject(DeliveryService);

  isAvailable    = signal(false);
  partnerName    = signal('Partner');
  togglingStatus = signal(false);

  navItems = [
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
}