import { Component, OnInit, inject, signal }
  from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { OrderService, OrderStatus }
  from '../../../core/services/order.service';
import { ToastService }
  from '../../../core/services/toast.service';

@Component({
  selector: 'app-order-history',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './order-history.component.html'
})
export class OrderHistoryComponent implements OnInit {
  private orderSvc = inject(OrderService);
  private toast    = inject(ToastService);
  router           = inject(Router);

  OrderStatus = OrderStatus;

  orders  = signal<any[]>([]);
  loading = signal(true);
  error   = signal(false);
  filter  = signal<string>('All');

  filters = ['All', 'Active', 'Delivered', 'Cancelled'];

  private readonly inactiveStatuses = [
    OrderStatus.Delivered, OrderStatus.Cancelled, OrderStatus.Refunded,
  ];

  filteredOrders = () => {
    const f = this.filter();
    if (f === 'All') return this.orders();
    if (f === 'Active') return this.orders().filter(o => !this.inactiveStatuses.includes(o.status));
    if (f === 'Delivered') return this.orders().filter(o => o.status === OrderStatus.Delivered);
    if (f === 'Cancelled') return this.orders().filter(o => o.status === OrderStatus.Cancelled);
    return this.orders();
  };

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    this.loading.set(true);
    this.error.set(false);
    this.orderSvc.getMyOrders().subscribe({
      next: orders => {
        this.orders.set(orders);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.error.set(true);
        this.toast.error('Failed to load your orders.');
      }
    });
  }

  trackOrder(orderId: string): void {
    this.router.navigate(['/orders', orderId, 'track']);
  }

  getStatusLabel(status: OrderStatus): string {
    const map: Record<OrderStatus, string> = {
      [OrderStatus.Pending]:        'Pending',
      [OrderStatus.Confirmed]:      'Confirmed',
      [OrderStatus.Preparing]:      'Preparing',
      [OrderStatus.Ready]:          'Ready',
      [OrderStatus.PickedUp]:       'Picked Up',
      [OrderStatus.OutForDelivery]: 'Out for Delivery',
      [OrderStatus.Delivered]:      'Delivered',
      [OrderStatus.Cancelled]:      'Cancelled',
      [OrderStatus.Refunded]:       'Refunded',
    };
    return map[status] ?? 'Unknown';
  }

  getStatusColor(status: OrderStatus): string {
    const map: Record<OrderStatus, string> = {
      [OrderStatus.Pending]:        'bg-yellow-100 text-yellow-700',
      [OrderStatus.Confirmed]:      'bg-blue-100 text-blue-700',
      [OrderStatus.Preparing]:      'bg-orange-100 text-orange-700',
      [OrderStatus.Ready]:          'bg-purple-100 text-purple-700',
      [OrderStatus.PickedUp]:       'bg-indigo-100 text-indigo-700',
      [OrderStatus.OutForDelivery]: 'bg-cyan-100 text-cyan-700',
      [OrderStatus.Delivered]:      'bg-green-100 text-green-700',
      [OrderStatus.Cancelled]:      'bg-red-100 text-red-700',
      [OrderStatus.Refunded]:       'bg-gray-100 text-gray-700',
    };
    return map[status] ?? 'bg-gray-100 text-gray-600';
  }

  getStatusIcon(status: OrderStatus): string {
    const map: Record<OrderStatus, string> = {
      [OrderStatus.Pending]:        '🕐',
      [OrderStatus.Confirmed]:      '✅',
      [OrderStatus.Preparing]:      '👨‍🍳',
      [OrderStatus.Ready]:          '📦',
      [OrderStatus.PickedUp]:       '🛵',
      [OrderStatus.OutForDelivery]: '🚀',
      [OrderStatus.Delivered]:      '🎉',
      [OrderStatus.Cancelled]:      '❌',
      [OrderStatus.Refunded]:       '💰',
    };
    return map[status] ?? '📋';
  }

  isActive(status: OrderStatus): boolean {
    return !this.inactiveStatuses.includes(status);
  }

  progressWidth(status: OrderStatus): string {
    const map: Record<OrderStatus, string> = {
      [OrderStatus.Pending]:        '15%',
      [OrderStatus.Confirmed]:      '30%',
      [OrderStatus.Preparing]:      '50%',
      [OrderStatus.Ready]:          '65%',
      [OrderStatus.PickedUp]:       '80%',
      [OrderStatus.OutForDelivery]: '90%',
      [OrderStatus.Delivered]:      '100%',
      [OrderStatus.Cancelled]:      '100%',
      [OrderStatus.Refunded]:       '100%',
    };
    return map[status] ?? '100%';
  }
}
