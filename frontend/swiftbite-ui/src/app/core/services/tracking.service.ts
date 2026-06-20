import { Injectable, inject } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from '../auth/auth.service';


export interface LocationUpdate {
  orderId:     string;
  latitude:    number;
  longitude:   number;
  partnerName: string;
  status:      string;
  updatedAt:   string;
}

@Injectable({ providedIn: 'root' })
export class TrackingService {
  private auth        = inject(AuthService);
  private connection: HubConnection | null = null;

  private locationSubject = new BehaviorSubject<LocationUpdate | null>(null);
  location$ = this.locationSubject.asObservable();

  async startTracking(orderId: string): Promise<void> {
    const token = this.auth.getToken();

    this.connection = new HubConnectionBuilder()
      .withUrl(`${environment.signalRHub}`, {
        accessTokenFactory: () => token ?? ''
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();

    // ✅ Listen for location updates
    this.connection.on('LocationUpdated', (data: LocationUpdate) => {
      this.locationSubject.next(data);
    });

    await this.connection.start();

    // ✅ Join order tracking group
    await this.connection.invoke('JoinOrderTracking', orderId);
    console.log(`🗺️ Tracking started for order ${orderId}`);
  }

  async stopTracking(orderId: string): Promise<void> {
    if (this.connection) {
      await this.connection.invoke('LeaveOrderTracking', orderId);
      await this.connection.stop();
      this.connection = null;
      this.locationSubject.next(null);
    }
  }
}