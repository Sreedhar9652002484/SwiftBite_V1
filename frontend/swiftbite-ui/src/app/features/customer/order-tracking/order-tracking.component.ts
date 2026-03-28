import { Component, OnInit, OnDestroy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { OrderService, Order, OrderStatus } from '../../../core/services/order.service';  // ✅ IMPORT ENUM
import { NotificationService } from '../../../core/services/notification.service';
import { ToastService } from '../../../core/services/toast.service';

// ✅ ADD: PaymentStatus enum to match backend
export enum PaymentStatus {
  Pending = 1,
  Paid = 2,
  Failed = 3,
  Refunded = 4
}

@Component({
  selector: 'app-order-tracking',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './order-tracking.component.html',
  styleUrl: './order-tracking.component.scss'
})
export class OrderTrackingComponent implements OnInit, OnDestroy {

  private route = inject(ActivatedRoute);
  public router = inject(Router);
  private orderSvc = inject(OrderService);
  private notifSvc = inject(NotificationService);
  private toast = inject(ToastService);

  order = signal<any>(null);
  loading = signal(true);
  orderId = '';

  // ✅ UPDATED: Use ENUM values in status field
  steps = [
    { status: OrderStatus.Pending, label: 'Order Placed', icon: '🛒', desc: 'Your order has been received' },
    { status: OrderStatus.Confirmed, label: 'Confirmed', icon: '✅', desc: 'Restaurant accepted your order' },
    { status: OrderStatus.Preparing, label: 'Preparing', icon: '👨‍🍳', desc: 'Chef is cooking your food' },
    { status: OrderStatus.Ready, label: 'Ready', icon: '📦', desc: 'Food is packed and ready' },
    { status: OrderStatus.PickedUp, label: 'Picked Up', icon: '🛵', desc: 'Delivery partner picked up' },
    { status: OrderStatus.OutForDelivery, label: 'Out for Delivery', icon: '🚀', desc: 'On the way to your door!' },
    { status: OrderStatus.Delivered, label: 'Delivered', icon: '🎉', desc: 'Enjoy your meal!' },
  ];

  // ✅ UPDATED: Use ENUM values for order
  statusOrder = [
    OrderStatus.Pending,
    OrderStatus.Confirmed,
    OrderStatus.Preparing,
    OrderStatus.Ready,
    OrderStatus.PickedUp,
    OrderStatus.OutForDelivery,
    OrderStatus.Delivered
  ];

  private pollInterval: any;

  ngOnInit(): void {
    this.orderId = this.route.snapshot.paramMap.get('id')!;
    this.loadOrder();
    this.connectSignalR();
    // ✅ Poll every 15s as fallback
    this.pollInterval = setInterval(() => this.loadOrder(), 15000);
  }

  ngOnDestroy(): void {
    clearInterval(this.pollInterval);
  }

  loadOrder(): void {
    this.orderSvc.getOrderById(this.orderId)
      .subscribe({
        next: order => {
          const prev = this.order()?.status;
          this.order.set(order);
          this.loading.set(false);
          // ✅ Toast on status change - Compare ENUM values
          if (prev && prev !== order.status) {
            this.onStatusChange(order.status);
          }
        },
        error: () => {
          this.loading.set(false);
          // Mock for demo
          this.order.set(this.getMockOrder());
        }
      });
  }

  connectSignalR(): void {
    // ✅ SignalR pushes status updates in real time!
    this.notifSvc.connectSignalR();
  }

  /**
   * ✅ UPDATED: Accept OrderStatus ENUM (number)
   */
  onStatusChange(status: OrderStatus): void {
    const step = this.steps.find(s => s.status === status);
    if (step) {
      this.toast.success(
        `${step.icon} ${step.label} — ${step.desc}`);
    }
  }

  /**
   * Get current step index based on ENUM value
   * ✅ UPDATED: Work with ENUM values
   */
  getCurrentStepIndex(): number {
    const currentStatus = this.order()?.status ?? OrderStatus.Pending;
    return this.statusOrder.indexOf(currentStatus);
  }

  /**
   * ✅ UPDATED: Compare ENUM values
   */
  isStepDone(status: OrderStatus): boolean {
    const current = this.getCurrentStepIndex();
    const idx = this.statusOrder.indexOf(status);
    return idx <= current;
  }

  /**
   * ✅ UPDATED: Compare ENUM values
   */
  isStepActive(status: OrderStatus): boolean {
    return this.order()?.status === status;
  }

  getProgressPercent(): number {
    const idx = this.getCurrentStepIndex();
    return Math.round((idx / (this.statusOrder.length - 1)) * 100);
  }

  /**
   * ✅ UPDATED: Check ENUM values for cancelled status
   */
  isCancelled(): boolean {
    const status = this.order()?.status;
    return status === OrderStatus.Cancelled || status === OrderStatus.Refunded;
  }

  /**
   * ✅ UPDATED: Check ENUM values for cancellable status
   */
  canCancel(): boolean {
    const status = this.order()?.status;
    return status === OrderStatus.Pending || status === OrderStatus.Confirmed;
  }

  cancelOrder(): void {
    if (!confirm('Are you sure you want to cancel this order?')) {
      return;
    }

    this.orderSvc.cancelOrder(this.orderId)
      .subscribe({
        next: () => {
          this.toast.success('Order cancelled.');
          this.loadOrder();
        },
        error: () => {
          this.toast.error('Cannot cancel order now.');
        }
      });
  }

  /**
   * Get payment status label
   * ✅ UPDATED: Handle PaymentStatus enum
   */
  getPaymentStatusLabel(status: PaymentStatus | number): string {
    const labels: Record<number, string> = {
      [PaymentStatus.Pending]: 'Pending',
      [PaymentStatus.Paid]: 'Paid',
      [PaymentStatus.Failed]: 'Failed',
      [PaymentStatus.Refunded]: 'Refunded',
    };
    return labels[status] || 'Unknown';
  }

  /**
   * Get order status label
   * ✅ NEW: Helper method for status labels
   */
  getOrderStatusLabel(status: OrderStatus): string {
    const labels: Record<OrderStatus, string> = {
      [OrderStatus.Pending]: 'Pending',
      [OrderStatus.Confirmed]: 'Confirmed',
      [OrderStatus.Preparing]: 'Preparing',
      [OrderStatus.Ready]: 'Ready',
      [OrderStatus.PickedUp]: 'Picked Up',
      [OrderStatus.OutForDelivery]: 'Out for Delivery',
      [OrderStatus.Delivered]: 'Delivered',
      [OrderStatus.Cancelled]: 'Cancelled',
      [OrderStatus.Refunded]: 'Refunded',
    };
    return labels[status] || 'Unknown';
  }

  /**
   * Mock order for demo
   * ✅ UPDATED: Use ENUM values
   */
  getMockOrder(): any {
    return {
      id: this.orderId,
      restaurantName: 'Paradise Biryani',
      status: OrderStatus.Preparing,  // ✅ Use ENUM
      paymentStatus: PaymentStatus.Paid,  // ✅ Use ENUM
      placedAt: new Date(),
      deliveryAddress: 'Kukatpally, Hyderabad',
      subTotal: 350,
      deliveryFee: 30,
      taxes: 17.5,
      totalAmount: 397.5,
      items: [
        {
          name: 'Chicken Biryani',
          quantity: 2,
          unitPrice: 175,
          totalPrice: 350
        }
      ],
      statusHistory: [
        {
          status: OrderStatus.Pending,
          note: 'Order placed',
          timestamp: new Date()
        },
        {
          status: OrderStatus.Confirmed,
          note: 'Restaurant confirmed',
          timestamp: new Date()
        },
        {
          status: OrderStatus.Preparing,
          note: 'Cooking started',
          timestamp: new Date()
        },
      ]
    };
  }
}