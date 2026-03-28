import { Injectable, signal, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import * as signalR from '@microsoft/signalr';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { AuthService } from '../auth/auth.service';
import { ApiResponseService } from './api-response.service';  // ✅ ADD

// ✅ ADD: NotificationType enum
export enum NotificationType {
  OrderPlaced = 1,
  OrderConfirmed = 2,
  OrderPreparing = 3,
  OrderReady = 4,
  OrderPickedUp = 5,
  OrderOutForDelivery = 6,
  OrderDelivered = 7,
  OrderCancelled = 8,
  PaymentSuccess = 9,
  PaymentFailed = 10,
  RestaurantUpdate = 11,
  Promotion = 12,
  General = 13
}

export interface Notification {
  id: string;
  userId: string;
  title: string;
  message: string;
  type: NotificationType;  // ✅ Use enum
  isRead: boolean;
  referenceId?: string;
  createdAt: Date;
}

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private http = inject(HttpClient);
  private authService = inject(AuthService);
  private apiResponseService = inject(ApiResponseService);  // ✅ ADD
  private api = environment.apiGatewayUrl;

  notifications = signal<Notification[]>([]);
  unreadCount = signal<number>(0);

  private hubConnection?: signalR.HubConnection;

  /**
   * Connect to SignalR hub
   */
  async connectSignalR(): Promise<void> {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(environment.signalRHub, {
        accessTokenFactory: () => this.authService.getToken() || ''
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    // 🔔 Listen for notifications from server
    this.hubConnection.on(
      'ReceiveNotification', (data: Notification) => {
        this.notifications.update(n => [data, ...n]);
        this.unreadCount.update(c => c + 1);
        this.showBrowserNotification(data.title, data.message);
      });

    this.hubConnection.onreconnected(() =>
      console.log('🔌 SignalR reconnected'));

    try {
      await this.hubConnection.start();
      console.log('✅ SignalR connected!');
      await this.loadNotifications();
    } catch (err) {
      console.error('SignalR failed:', err);
      this.loadMockNotifications(); // ✅ Fallback
    }
  }

  /**
   * Disconnect from SignalR hub
   */
  async disconnectSignalR(): Promise<void> {
    await this.hubConnection?.stop();
  }

  /**
   * Load notifications from backend
   * ✅ UPDATED: Extracts data from ApiResponse
   */
  loadNotifications(): void {
    this.http.get<any>(`${this.api}/api/notifications`)
      .pipe(
        map(response => {
          // ✅ Extract data from ApiResponse
          return this.apiResponseService.extractData<{
            notifications: Notification[];
            unreadCount: number;
          }>(response);
        })
      )
      .subscribe({
        next: (result) => {
          if (result) {
            this.notifications.set(result.notifications || []);
            this.unreadCount.set(result.unreadCount || 0);
          }
        },
        error: (err) => {
          console.error('Failed to load notifications:', err);
          this.loadMockNotifications();
        }
      });
  }

  /**
   * Load mock notifications (fallback)
   */
  loadMockNotifications(): void {
    this.notifications.set([
      {
        id: '1',
        userId: 'user-1',
        title: '🎉 Order Delivered!',
        message: 'Your Paradise Biryani order delivered!',
        type: 1,
        isRead: false,
        referenceId: 'ord-001',
        createdAt: new Date()
      },
      {
        id: '2',
        userId: 'user-1',
        title: '💳 Payment Successful!',
        message: '₹397.50 paid successfully.',
        type: 2,
        isRead: false,
        referenceId: 'ord-001',
        createdAt: new Date(Date.now() - 3600000)
      }
    ]);
    this.unreadCount.set(2);
  }

  /**
   * Mark all notifications as read
   * ✅ UPDATED: Handles ApiResponse
   */
  markAllRead(): void {
    this.http.put<any>(
      `${this.api}/api/notifications/mark-all-read`, {})
      .pipe(
        map(response => {
          if (!this.apiResponseService.isSuccess(response)) {
            throw new Error('Failed to mark as read');
          }
          return undefined;
        })
      )
      .subscribe({
        next: () => {
          this.unreadCount.set(0);
          this.notifications.update(list =>
            list.map(n => ({ ...n, isRead: true })));
        },
        error: (err) => console.error('Error marking as read:', err)
      });
  }

  /**
   * Show browser notification
   */
  private showBrowserNotification(title: string, body: string): void {
    if (Notification.permission === 'granted') {
      new Notification(title, {
        body,
        icon: '/assets/logo.png'
      });
    }
  }

  /**
   * Request browser notification permission
   */
  requestNotificationPermission(): void {
    Notification.requestPermission();
  }
}