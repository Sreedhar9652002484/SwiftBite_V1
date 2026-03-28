import { Component, OnInit, OnDestroy,
  inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { NotificationService,  NotificationType }
  from '../../../../core/services/notification.service';
import { AuthService }
  from '../../../../core/auth/auth.service';

@Component({
  selector: 'app-notification-bell',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="relative">

      <!-- Bell Button -->
      <button
        (click)="toggleDropdown()"
        class="relative p-2 rounded-xl hover:bg-gray-100
               transition-all">
        <span class="text-xl">🔔</span>

        <!-- Unread Badge -->
        @if (notifSvc.unreadCount() > 0) {
          <span class="absolute -top-1 -right-1
                       bg-red-500 text-white text-xs
                       font-bold rounded-full min-w-5 h-5
                       flex items-center justify-center
                       px-1 animate-pulse">
            {{ notifSvc.unreadCount() > 99
               ? '99+' : notifSvc.unreadCount() }}
          </span>
        }
      </button>

      <!-- Dropdown -->
      @if (isOpen()) {
        <!-- Backdrop -->
        <div class="fixed inset-0 z-40"
          (click)="isOpen.set(false)">
        </div>

        <!-- Panel -->
        <div class="absolute right-0 top-12 w-80
                    bg-white rounded-2xl shadow-2xl
                    border border-gray-100 z-50
                    overflow-hidden">

          <!-- Header -->
          <div class="flex items-center justify-between
                      px-4 py-3 border-b bg-gray-50">
            <h3 class="font-extrabold text-gray-900">
              🔔 Notifications
            </h3>
            <div class="flex items-center gap-2">
              @if (notifSvc.unreadCount() > 0) {
                <button
                  (click)="markAllRead()"
                  class="text-xs text-orange-500
                         font-semibold hover:text-orange-600">
                  Mark all read
                </button>
              }
              <button (click)="isOpen.set(false)"
                class="text-gray-400 hover:text-gray-600
                       text-lg leading-none">
                ×
              </button>
            </div>
          </div>

          <!-- Notifications List -->
          <div class="max-h-96 overflow-y-auto">

            @if (notifSvc.notifications().length === 0) {
              <div class="text-center py-10">
                <div class="text-4xl mb-2">🔕</div>
                <p class="text-gray-400 text-sm">
                  No notifications yet
                </p>
              </div>
            }

            @for (n of notifSvc.notifications();
                  track n.id ?? $index) {
              <div
                class="px-4 py-3 border-b border-gray-50
                       hover:bg-gray-50 transition-all
                       cursor-pointer"
                [class.bg-orange-50]="!n.isRead"
                (click)="onNotificationClick(n)">

                <div class="flex gap-3">
                  <!-- Icon -->
                  <div class="w-9 h-9 rounded-full
                              flex items-center
                              justify-center text-lg
                              flex-shrink-0"
                    [class]="!n.isRead
                      ? 'bg-orange-100' : 'bg-gray-100'">
                    {{ getIcon(n.type) }}
                  </div>

                  <div class="flex-1 min-w-0">
                    <div class="flex items-start
                                justify-between gap-1">
                      <p class="text-sm font-semibold
                                 text-gray-900 leading-snug">
                        {{ n.title }}
                      </p>
                      <!-- Unread dot -->
                      @if (!n.isRead) {
                        <div class="w-2 h-2 bg-orange-500
                                    rounded-full flex-shrink-0
                                    mt-1"></div>
                      }
                    </div>
                    <p class="text-xs text-gray-500
                               mt-0.5 leading-snug">
                      {{ n.message }}
                    </p>
                    <p class="text-xs text-gray-300 mt-1">
                      {{ getTimeAgo(n.createdAt) }}
                    </p>
                  </div>
                </div>
              </div>
            }
          </div>

          <!-- Footer -->
          <div class="px-4 py-3 bg-gray-50 border-t">
            <button
              (click)="isOpen.set(false)"
              class="w-full text-center text-sm
                     text-orange-500 font-semibold
                     hover:text-orange-600">
              Close
            </button>
          </div>
        </div>
      }
    </div>
  `
})
export class NotificationBellComponent
  implements OnInit, OnDestroy {

  notifSvc   = inject(NotificationService);
  private authSvc = inject(AuthService);
  private router  = inject(Router);

  isOpen = signal(false);

  ngOnInit(): void {
    // ✅ Connect SignalR when component loads
    if (this.authSvc.isLoggedIn()) {
      this.notifSvc.connectSignalR();
      this.notifSvc.requestNotificationPermission();
    }
  }

  ngOnDestroy(): void {
    this.notifSvc.disconnectSignalR();
  }

  toggleDropdown(): void {
    this.isOpen.update(v => !v);
  }

  markAllRead(): void {
    this.notifSvc.markAllRead();
  }

  onNotificationClick(n: any): void {
    this.isOpen.set(false);
    // Navigate based on type
    if (n.referenceId) {
      if (n.type?.includes('Order') ||
          n.type?.includes('Payment')) {
        this.router.navigate(
          ['/orders', n.referenceId, 'track']);
      }
    }
  }

 getIcon(type: NotificationType): string {
    const iconMap: Record<string, string> = {
      // String-enum values
      'OrderPlaced':    '📦',
      'OrderConfirmed': '✅',
      'OrderPreparing': '👨‍🍳',
      'OutForDelivery': '🛵',
      'OrderDelivered': '🎉',
      'OrderCancelled': '❌',
      'Promotion':      '🎁',
      'General':        '🔔',
      // Numeric-enum values (if backend sends numbers)
      '0':  '📦',
      '1':  '✅',
      '2':  '👨‍🍳',
      '3':  '🛵',
      '4':  '🎉',
      '5':  '❌',
      '6':  '🎁',
      '7':  '🔔',
    };
    return iconMap[String(type)] ?? '🔔';
  }

  getTimeAgo(date: Date | string): string {
    if (!date) return '';
    const now   = new Date();
    const then  = new Date(date);
    const diff  = Math.floor(
      (now.getTime() - then.getTime()) / 1000);

    if (diff < 60)     return 'Just now';
    if (diff < 3600)
      return `${Math.floor(diff / 60)}m ago`;
    if (diff < 86400)
      return `${Math.floor(diff / 3600)}h ago`;
    return `${Math.floor(diff / 86400)}d ago`;
  }
}