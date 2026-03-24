import { Component, OnInit, inject, signal }
  from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { OrderService }
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

  orders  = signal<any[]>([]);
  loading = signal(true);
  filter  = signal<string>('All');

  filters = ['All', 'Active', 'Delivered', 'Cancelled'];

  filteredOrders = () => {
    const f = this.filter();
    if (f === 'All') return this.orders();
    if (f === 'Active') return this.orders().filter(o =>
      !['Delivered','Cancelled','Refunded']
        .includes(o.status));
    return this.orders().filter(o =>
      o.status === f);
  };

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    this.orderSvc.getMyOrders().subscribe({
      next: orders => {
        this.orders.set(orders);
        this.loading.set(false);
      },
      error: () => {
        this.orders.set(this.getMockOrders());
        this.loading.set(false);
      }
    });
  }

  trackOrder(orderId: string): void {
    this.router.navigate(['/orders', orderId, 'track']);
  }

  getStatusColor(status: string): string {
    const map: Record<string, string> = {
      'Pending':        'bg-yellow-100 text-yellow-700',
      'Confirmed':      'bg-blue-100 text-blue-700',
      'Preparing':      'bg-orange-100 text-orange-700',
      'Ready':          'bg-purple-100 text-purple-700',
      'PickedUp':       'bg-indigo-100 text-indigo-700',
      'OutForDelivery': 'bg-cyan-100 text-cyan-700',
      'Delivered':      'bg-green-100 text-green-700',
      'Cancelled':      'bg-red-100 text-red-700',
      'Refunded':       'bg-gray-100 text-gray-700',
    };
    return map[status] ?? 'bg-gray-100 text-gray-600';
  }

  getStatusIcon(status: string): string {
    const map: Record<string, string> = {
      'Pending':        '🕐',
      'Confirmed':      '✅',
      'Preparing':      '👨‍🍳',
      'Ready':          '📦',
      'PickedUp':       '🛵',
      'OutForDelivery': '🚀',
      'Delivered':      '🎉',
      'Cancelled':      '❌',
      'Refunded':       '💰',
    };
    return map[status] ?? '📋';
  }

  isActive(status: string): boolean {
    return !['Delivered', 'Cancelled', 'Refunded']
      .includes(status);
  }

  getMockOrders(): any[] {
    return [
      {
        id: 'ord-001-mock',
        restaurantName: 'Paradise Biryani',
        status: 'Preparing',
        totalAmount: 397.5,
        placedAt: new Date(),
        items: [
          { name: 'Chicken Biryani', quantity: 2 }
        ]
      },
      {
        id: 'ord-002-mock',
        restaurantName: 'Pizza Hut',
        status: 'Delivered',
        totalAmount: 549,
        placedAt: new Date(
          Date.now() - 86400000), // yesterday
        items: [
          { name: 'Margherita Pizza', quantity: 1 },
          { name: 'Garlic Bread',     quantity: 1 }
        ]
      },
      {
        id: 'ord-003-mock',
        restaurantName: "McDonald's",
        status: 'Cancelled',
        totalAmount: 280,
        placedAt: new Date(
          Date.now() - 172800000), // 2 days ago
        items: [
          { name: 'McAloo Tikki', quantity: 2 }
        ]
      }
    ];
  }
}