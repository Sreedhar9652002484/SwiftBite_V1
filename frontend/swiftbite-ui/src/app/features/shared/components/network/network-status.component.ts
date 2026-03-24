import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-network-status',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (!isOnline()) {
      <div class="offline-banner">
        📡 No internet connection —
        Some features may not work
      </div>
    }
    @if (justReconnected()) {
      <div class="online-banner">
        ✅ Back online!
      </div>
    }
  `,
  styles: [`
    .offline-banner {
      position: fixed; top: 0; left: 0; right: 0;
      background: #FEF2F2; color: #991B1B;
      border-bottom: 2px solid #FECACA;
      text-align: center; padding: 10px;
      font-size: 13px; font-weight: 600;
      z-index: 9997; animation: slideDown 0.3s ease;
    }
    .online-banner {
      position: fixed; top: 0; left: 0; right: 0;
      background: #ECFDF5; color: #065F46;
      border-bottom: 2px solid #A7F3D0;
      text-align: center; padding: 10px;
      font-size: 13px; font-weight: 600;
      z-index: 9997; animation: slideDown 0.3s ease;
    }
    @keyframes slideDown {
      from { transform: translateY(-100%); }
      to   { transform: translateY(0); }
    }
  `]
})
export class NetworkStatusComponent implements OnInit {
  isOnline        = signal(true);
  justReconnected = signal(false);

  ngOnInit(): void {
    this.isOnline.set(navigator.onLine);

    window.addEventListener('offline', () => {
      this.isOnline.set(false);
      this.justReconnected.set(false);
    });

    window.addEventListener('online', () => {
      this.isOnline.set(true);
      this.justReconnected.set(true);
      setTimeout(() =>
        this.justReconnected.set(false), 3000);
    });
  }
}