import { Injectable, signal, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import { AuthService } from '../auth/auth.service';


@Injectable({ providedIn: 'root' })
export class NotificationService {
  private http        = inject(HttpClient);
  private authService = inject(AuthService);
  private api         = environment.apiGatewayUrl;

  notifications  = signal<any[]>([]);
  unreadCount    = signal<number>(0);

  private hubConnection?: signalR.HubConnection;

  // ✅ Call this after login!
  async connectSignalR(): Promise<void> {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(environment.signalRHub, {
        accessTokenFactory: () =>
          this.authService.getToken()
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    // 🔔 Listen for notifications from server
    this.hubConnection.on(
      'ReceiveNotification', (data: any) => {
        this.notifications.update(n => [data, ...n]);
        this.unreadCount.update(c => c + 1);
        this.showBrowserNotification(
          data.title, data.message);
      });

    this.hubConnection.onreconnected(() =>
      console.log('🔌 SignalR reconnected'));

    try {
      await this.hubConnection.start();
      console.log('✅ SignalR connected!');
      await this.loadNotifications();
    } catch (err) {
      console.error('SignalR failed:', err);
       this.loadMockNotifications(); // ✅ Show mock data
    }
  }

  async disconnectSignalR(): Promise<void> {
    await this.hubConnection?.stop();
  }

  loadNotifications(): void {
    this.http.get<any>(
      `${this.api}/api/notifications`)
      .subscribe(result => {
        this.notifications.set(result.notifications);
        this.unreadCount.set(result.unreadCount);
      });
  }

  // Add this method and call it in connectSignalR on error:
loadMockNotifications(): void {
  this.notifications.set([
    {
      id: '1',
      title: '🎉 Order Delivered!',
      message: 'Your Paradise Biryani order delivered!',
      type: 'OrderDelivered',
      isRead: false,
      referenceId: 'ord-001',
      createdAt: new Date()
    },
    {
      id: '2',
      title: '💳 Payment Successful!',
      message: '₹397.50 paid successfully.',
      type: 'PaymentSuccess',
      isRead: false,
      referenceId: 'ord-001',
      createdAt: new Date(Date.now() - 3600000)
    },
    {
      id: '3',
      title: '✅ Order Confirmed!',
      message: 'Pizza Hut accepted your order!',
      type: 'OrderConfirmed',
      isRead: true,
      referenceId: 'ord-002',
      createdAt: new Date(Date.now() - 7200000)
    }
  ]);
  this.unreadCount.set(2);
}

  markAllRead(): void {
    this.http.put(
      `${this.api}/api/notifications/mark-all-read`, {})
      .subscribe(() => {
        this.unreadCount.set(0);
        this.notifications.update(list =>
          list.map(n => ({ ...n, isRead: true })));
      });
  }

  private showBrowserNotification(
    title: string, body: string): void {
    if (Notification.permission === 'granted') {
      new Notification(title, { body,
        icon: '/assets/logo.png' });
    }
  }

  requestNotificationPermission(): void {
    Notification.requestPermission();
  }
  
}