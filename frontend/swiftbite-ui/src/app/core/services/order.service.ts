import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ApiResponseService } from './api-response.service';  // ✅ ADD

export interface OrderItem {
  id: string;
  menuItemId: string;
  name: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
  imageUrl?: string;
}
// ✅ EXACT MATCH WITH BACKEND ENUM
export enum OrderStatus {
  Pending = 1,
  Confirmed = 2,
  Preparing = 3,
  Ready = 4,
  PickedUp = 5,
  OutForDelivery = 6,
  Delivered = 7,
  Cancelled = 8,
  Refunded = 9
}

// ✅ ADD: PaymentStatus enum
export enum PaymentStatus {
  Pending = 1,
  Paid = 2,
  Failed = 3,
  Refunded = 4
}
export interface OrderItem {
  id: string;
  menuItemId: string;
  name: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
  imageUrl?: string;
}
export interface Order {
  id: string;
  customerId: string;
  customerName: string;
  customerPhone: string;
  restaurantId: string;
  restaurantName: string;
  deliveryAddress: string;
  deliveryCity: string;
  deliveryPinCode: string;
  subTotal: number;
  deliveryFee: number;
  taxes: number;
  discount: number;
  totalAmount: number;
  status: OrderStatus;  
  paymentStatus: PaymentStatus;
  paymentMethod: string;
  specialInstructions?: string;
  placedAt: Date;
  estimatedDeliveryAt?: Date;
  deliveredAt?: Date;
  items: OrderItem[];
  rowVersion: string;
}

@Injectable({ providedIn: 'root' })
export class OrderService {
  private http = inject(HttpClient);
  private apiResponseService = inject(ApiResponseService);  // ✅ ADD
  private api = environment.apiGatewayUrl;

  /**
   * Place order
   * ✅ UPDATED: Extracts data from ApiResponse<Order>
   */
  placeOrder(request: any): Observable<Order> {
    return this.http.post<any>(`${this.api}/api/orders`, request)
      .pipe(
        map(response => {
          const order = this.apiResponseService.extractData<Order>(response);
          if (!order) {
            throw new Error('Failed to place order');
          }
          return order;
        })
      );
  }

  /**
   * Get my orders
   * ✅ UPDATED: Extracts data from ApiResponse<Order[]>
   */
  getMyOrders(): Observable<Order[]> {
    return this.http.get<any>(`${this.api}/api/orders/my`)
      .pipe(
        map(response => {
          const orders = this.apiResponseService.extractData<Order[]>(response);
          return orders || [];
        })
      );
  }

  /**
   * Get order by ID
   * ✅ UPDATED: Extracts data from ApiResponse<Order>
   */
  getOrderById(id: string): Observable<Order> {
    return this.http.get<any>(`${this.api}/api/orders/${id}`)
      .pipe(
        map(response => {
          const order = this.apiResponseService.extractData<Order>(response);
          if (!order) {
            throw new Error('Order not found');
          }
          return order;
        })
      );
  }

  /**
   * Cancel order
   * ✅ UPDATED: Handles ApiResponse without data
   */
  cancelOrder(id: string): Observable<void> {
    return this.http.post<any>(`${this.api}/api/orders/${id}/cancel`, {})
      .pipe(
        map(response => {
          if (!this.apiResponseService.isSuccess(response)) {
            throw new Error('Failed to cancel order');
          }
          return undefined;
        })
      );
  }

  /**
   * Update order status
   * ✅ UPDATED: Extracts data from ApiResponse<Order>
   */
updateStatus(order: Order, newStatus: number): Observable<Order> {
  return this.http.put<any>(
    `${this.api}/api/orders/${order.id}/status`,
    {
      newStatus,
      rowVersion: order.rowVersion   // ✅ FIXED
    }
  ).pipe(
    map(response => {
      const updated = this.apiResponseService.extractData<Order>(response);
      if (!updated) {
        throw new Error('Failed to update order status');
      }
      return updated;
    })
  );
}

  /**
   * Get restaurant orders
   * ✅ UPDATED: Extracts data from ApiResponse<Order[]>
   */
  getRestaurantOrders(restaurantId: string): Observable<Order[]> {
    return this.http.get<any>(
      `${this.api}/api/orders/restaurant/${restaurantId}`)
      .pipe(
        map(response => {
          const orders = this.apiResponseService.extractData<Order[]>(response);
          return orders || [];
        })
      );
  }
}