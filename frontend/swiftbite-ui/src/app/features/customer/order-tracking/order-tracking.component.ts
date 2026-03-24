import { Component, OnInit, OnDestroy,
  inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { OrderService }
  from '../../../core/services/order.service';
import { NotificationService }
  from '../../../core/services/notification.service';
import { ToastService }
  from '../../../core/services/toast.service';

@Component({
  selector: 'app-order-tracking',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './order-tracking.component.html',
  styleUrl:    './order-tracking.component.scss'
})
export class OrderTrackingComponent
  implements OnInit, OnDestroy {

  private route       = inject(ActivatedRoute);
  public router      = inject(Router);
  private orderSvc    = inject(OrderService);
  private notifSvc    = inject(NotificationService);
  private toast       = inject(ToastService);

  order   = signal<any>(null);
  loading = signal(true);
  orderId = '';

  // ✅ All 7 steps of order lifecycle
  steps = [
    { status: 'Pending',
      label:  'Order Placed',
      icon:   '🛒',
      desc:   'Your order has been received' },
    { status: 'Confirmed',
      label:  'Confirmed',
      icon:   '✅',
      desc:   'Restaurant accepted your order' },
    { status: 'Preparing',
      label:  'Preparing',
      icon:   '👨‍🍳',
      desc:   'Chef is cooking your food' },
    { status: 'Ready',
      label:  'Ready',
      icon:   '📦',
      desc:   'Food is packed and ready' },
    { status: 'PickedUp',
      label:  'Picked Up',
      icon:   '🛵',
      desc:   'Delivery partner picked up' },
    { status: 'OutForDelivery',
      label:  'Out for Delivery',
      icon:   '🚀',
      desc:   'On the way to your door!' },
    { status: 'Delivered',
      label:  'Delivered',
      icon:   '🎉',
      desc:   'Enjoy your meal!' },
  ];

  statusOrder = [
    'Pending', 'Confirmed', 'Preparing',
    'Ready', 'PickedUp', 'OutForDelivery', 'Delivered'
  ];

  // Poll interval for live updates
  private pollInterval: any;

  ngOnInit(): void {
    this.orderId =
      this.route.snapshot.paramMap.get('id')!;
    this.loadOrder();
    this.connectSignalR();
    // ✅ Poll every 15s as fallback
    this.pollInterval = setInterval(
      () => this.loadOrder(), 15000);
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
          // ✅ Toast on status change
          if (prev && prev !== order.status)
            this.onStatusChange(order.status);
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

  onStatusChange(status: string): void {
    const step = this.steps.find(
      s => s.status === status);
    if (step)
      this.toast.success(
        `${step.icon} ${step.label} — ${step.desc}`);
  }

  getCurrentStepIndex(): number {
    return this.statusOrder.indexOf(
      this.order()?.status ?? 'Pending');
  }

  isStepDone(status: string): boolean {
    const current = this.getCurrentStepIndex();
    const idx = this.statusOrder.indexOf(status);
    return idx <= current;
  }

  isStepActive(status: string): boolean {
    return this.order()?.status === status;
  }

  getProgressPercent(): number {
    const idx = this.getCurrentStepIndex();
    return Math.round(
      (idx / (this.statusOrder.length - 1)) * 100);
  }

  isCancelled(): boolean {
    return ['Cancelled', 'Refunded']
      .includes(this.order()?.status);
  }

  canCancel(): boolean {
    return ['Pending', 'Confirmed']
      .includes(this.order()?.status);
  }

  cancelOrder(): void {
    if (!confirm(
      'Are you sure you want to cancel this order?'))
      return;
    this.orderSvc.cancelOrder(this.orderId)
      .subscribe({
        next: () => {
          this.toast.success('Order cancelled.');
          this.loadOrder();
        },
        error: () =>
          this.toast.error('Cannot cancel order now.')
      });
  }

  getMockOrder(): any {
    return {
      id: this.orderId,
      restaurantName: 'Paradise Biryani',
      status: 'Preparing',
      paymentStatus: 'Paid',
      placedAt: new Date(),
      deliveryAddress: 'Kukatpally, Hyderabad',
      subTotal: 350, deliveryFee: 30,
      taxes: 17.5, totalAmount: 397.5,
      items: [
        { name: 'Chicken Biryani',
          quantity: 2, unitPrice: 175, totalPrice: 350 }
      ],
      statusHistory: [
        { status: 'Pending',
          note: 'Order placed',
          timestamp: new Date() },
        { status: 'Confirmed',
          note: 'Restaurant confirmed',
          timestamp: new Date() },
        { status: 'Preparing',
          note: 'Cooking started',
          timestamp: new Date() },
      ]
    };
  }
}