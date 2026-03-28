import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ApiResponseService } from './api-response.service';  // ✅ ADD

export interface Payment {
  id: string;
  orderId: string;
  customerId: string;
  amount: number;
  currency: string;
  razorpayOrderId?: string;
  razorpayPaymentId?: string;
  status: number;
  method: string;
  failureReason?: string;
  refundAmount: number;
  createdAt: Date;
  paidAt?: Date;
  refundedAt?: Date;
}

export interface InitiatePaymentRequest {
  orderId: string;
  customerName: string;
  customerEmail: string;
  customerPhone: string;
  amount: number;
  method: string;
}

export interface VerifyPaymentRequest {
  razorpayOrderId: string;
  razorpayPaymentId: string;
  razorpaySignature: string;
}

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private http = inject(HttpClient);
  private apiResponseService = inject(ApiResponseService);  // ✅ ADD
  private api = environment.apiGatewayUrl;

  /**
   * Initiate payment
   * ✅ UPDATED: Extracts data from ApiResponse<Payment>
   */
  initiatePayment(request: InitiatePaymentRequest): Observable<Payment> {
    return this.http.post<any>(`${this.api}/api/payments/initiate`, request)
      .pipe(
        map(response => {
          const payment = this.apiResponseService.extractData<Payment>(response);
          if (!payment) {
            throw new Error('Failed to initiate payment');
          }
          return payment;
        })
      );
  }

  /**
   * Verify payment
   * ✅ UPDATED: Extracts data from ApiResponse<Payment>
   */
  verifyPayment(request: VerifyPaymentRequest): Observable<Payment> {
    return this.http.post<any>(`${this.api}/api/payments/verify`, request)
      .pipe(
        map(response => {
          const payment = this.apiResponseService.extractData<Payment>(response);
          if (!payment) {
            throw new Error('Payment verification failed');
          }
          return payment;
        })
      );
  }

  /**
   * Get my payments
   * ✅ UPDATED: Extracts data from ApiResponse<Payment[]>
   */
  getMyPayments(): Observable<Payment[]> {
    return this.http.get<any>(`${this.api}/api/payments/my`)
      .pipe(
        map(response => {
          const payments = this.apiResponseService.extractData<Payment[]>(response);
          return payments || [];
        })
      );
  }

  /**
   * Get payment by order ID
   * ✅ UPDATED: Extracts data from ApiResponse<Payment>
   */
  getPaymentByOrderId(orderId: string): Observable<Payment> {
    return this.http.get<any>(`${this.api}/api/payments/order/${orderId}`)
      .pipe(
        map(response => {
          const payment = this.apiResponseService.extractData<Payment>(response);
          if (!payment) {
            throw new Error('Payment not found');
          }
          return payment;
        })
      );
  }

  /**
   * Refund payment
   * ✅ UPDATED: Extracts data from ApiResponse<Payment>
   */
  refundPayment(orderId: string, refundAmount: number): Observable<Payment> {
    return this.http.post<any>(`${this.api}/api/payments/refund`, {
      orderId,
      refundAmount
    })
      .pipe(
        map(response => {
          const payment = this.apiResponseService.extractData<Payment>(response);
          if (!payment) {
            throw new Error('Refund failed');
          }
          return payment;
        })
      );
  }
}