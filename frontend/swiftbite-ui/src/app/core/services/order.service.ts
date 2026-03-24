import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class OrderService {
  private http = inject(HttpClient);
  private api  = environment.apiGatewayUrl;

  placeOrder(request: any): Observable<any> {
    return this.http.post(
      `${this.api}/api/orders`, request);
  }

  getMyOrders(): Observable<any[]> {
    return this.http.get<any[]>(
      `${this.api}/api/orders/my`);
  }

  getOrderById(id: string): Observable<any> {
    return this.http.get<any>(
      `${this.api}/api/orders/${id}`);
  }

  cancelOrder(id: string): Observable<any> {
    return this.http.delete(
      `${this.api}/api/orders/${id}`);
  }

  updateStatus(id: string, status: string): Observable<any> {
    return this.http.put(
      `${this.api}/api/orders/${id}/status`,
      { newStatus: status });
  }

  // ── Owner endpoint (new) ──────────────────────────────────
  // Fetches all orders for a specific restaurant
  // Backend: GET /api/orders/restaurant/{restaurantId}
  getRestaurantOrders(restaurantId: string): Observable<any[]> {
    return this.http.get<any[]>(
      `${this.api}/api/orders/restaurant/${restaurantId}`);
  }
}