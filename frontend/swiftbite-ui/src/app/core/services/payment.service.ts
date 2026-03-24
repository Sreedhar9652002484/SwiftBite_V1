import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private http = inject(HttpClient);
  private api  = environment.apiGatewayUrl;

  initiatePayment(request: any): Observable<any> {
    return this.http.post(
      `${this.api}/api/payments/initiate`, request);
  }

  verifyPayment(request: any): Observable<any> {
    return this.http.post(
      `${this.api}/api/payments/verify`, request);
  }

  getMyPayments(): Observable<any[]> {
    return this.http.get<any[]>(
      `${this.api}/api/payments/my`);
  }

  getPaymentByOrderId(orderId: string): Observable<any> {
    return this.http.get<any>(
      `${this.api}/api/payments/order/${orderId}`);
  }
}