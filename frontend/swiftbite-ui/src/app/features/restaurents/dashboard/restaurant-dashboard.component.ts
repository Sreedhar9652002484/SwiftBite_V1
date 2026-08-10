import { Component, OnInit, OnDestroy, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/auth/auth.service';
import { OrderService, Order, OrderStatus } from '../../../core/services/order.service';  // ✅ IMPORT ENUM

@Component({
  selector: 'app-restaurant-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './restaurant-dashboard.component.html',
  styleUrls: ['./restaurant-dashboard.component.scss'],
})
export class RestaurantDashboardComponent implements OnInit, OnDestroy {

  orders = signal<Order[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);
  updatingId = signal<string | null>(null);
  activeFilter = signal<OrderStatus | 'All'>(OrderStatus.Pending);  // ✅ USE ENUM
  private pollInterval: any;

  filteredOrders = computed(() => {
    const f = this.activeFilter();
    return f === 'All'
      ? this.orders()
      : this.orders().filter(o => o.status === f);
  });

  pendingCount = computed(() =>
    this.orders().filter(o => o.status === OrderStatus.Pending).length);

  activeCount = computed(() =>
    this.orders().filter(o =>
      o.status === OrderStatus.Confirmed ||
      o.status === OrderStatus.Preparing).length);

  // ✅ UPDATED: Map ENUM values to filter options
  filters: Array<{ label: string; value: OrderStatus | 'All' }> = [
    { label: 'All', value: 'All' },
    { label: 'Pending', value: OrderStatus.Pending },
    { label: 'Confirmed', value: OrderStatus.Confirmed },
    { label: 'Preparing', value: OrderStatus.Preparing },
    { label: 'Ready', value: OrderStatus.Ready },
    { label: 'Picked Up', value: OrderStatus.PickedUp },
    { label: 'Out for Delivery', value: OrderStatus.OutForDelivery },
    { label: 'Delivered', value: OrderStatus.Delivered },
    { label: 'Cancelled', value: OrderStatus.Cancelled },
  ];

  // ✅ UPDATED: Map ENUM values to labels
  statusLabels: Record<OrderStatus, string> = {
    [OrderStatus.Pending]: 'Pending',
    [OrderStatus.Confirmed]: 'Confirmed',
    [OrderStatus.Preparing]: 'Preparing',
    [OrderStatus.Ready]: 'Ready for Pickup',
    [OrderStatus.PickedUp]: 'Picked Up',
    [OrderStatus.OutForDelivery]: 'Out for Delivery',
    [OrderStatus.Delivered]: 'Delivered',
    [OrderStatus.Cancelled]: 'Cancelled',
    [OrderStatus.Refunded]: 'Refunded',
  };

  // ✅ NEW: Status order for sorting
  statusOrder: Record<OrderStatus, number> = {
    [OrderStatus.Pending]: 1,
    [OrderStatus.Confirmed]: 2,
    [OrderStatus.Preparing]: 3,
    [OrderStatus.Ready]: 4,
    [OrderStatus.PickedUp]: 5,
    [OrderStatus.OutForDelivery]: 6,
    [OrderStatus.Delivered]: 7,
    [OrderStatus.Cancelled]: 8,
    [OrderStatus.Refunded]: 9,
  };

  constructor(
    private auth: AuthService,
    private orderSvc: OrderService,
  ) {}

  ngOnInit(): void {
    this.loadOrders();
    this.pollInterval = setInterval(() => this.loadOrders(), 30_000);
  }

  ngOnDestroy(): void {
    clearInterval(this.pollInterval);
  }

  private getRestaurantId(): string | null {
    const user = this.auth.currentUser();
    return user?.['restaurantId']
      ?? user?.['restaurant_id']
      ?? user?.['RestaurantId']
      ?? null;
  }

  loadOrders(): void {
    const rid = this.getRestaurantId();
    if (!rid) {
      this.error.set('Restaurant ID not found in your profile.');
      this.loading.set(false);
      return;
    }

    this.orderSvc.getRestaurantOrders(rid).subscribe({
      next: orders => {
        // ✅ UPDATED: Sort by ENUM status values
        const sorted = [...orders].sort((a, b) => {
          // Pending orders first
          if (a.status === OrderStatus.Pending && b.status !== OrderStatus.Pending) return -1;
          if (b.status === OrderStatus.Pending && a.status !== OrderStatus.Pending) return 1;

          // Then by status order
          const aOrder = this.statusOrder[a.status] ?? 99;
          const bOrder = this.statusOrder[b.status] ?? 99;
          if (aOrder !== bOrder) return aOrder - bOrder;

          // Finally by date (newest first)
          return new Date(b.placedAt).getTime() - new Date(a.placedAt).getTime();
        });

        this.orders.set(sorted);
        this.loading.set(false);
        this.error.set(null);

        console.log('✅ Orders loaded:', orders);
      },
      error: err => {
        this.error.set('Failed to load orders. Please try again.');
        this.loading.set(false);
        console.error(err);
      },
    });
  }

  setFilter(f: OrderStatus | 'All'): void {
    this.activeFilter.set(f);
  }

  // ✅ UPDATED: Use ENUM values
  acceptOrder(order: Order): void { this.changeStatus(order, OrderStatus.Confirmed); }
  rejectOrder(order: Order): void { this.changeStatus(order, OrderStatus.Cancelled); }
  markPreparing(order: Order): void { this.changeStatus(order, OrderStatus.Preparing); }
  markReady(order: Order): void { this.changeStatus(order, OrderStatus.Ready); }

  // ✅ UPDATED: Accept OrderStatus ENUM
 private changeStatus(order: Order, status: OrderStatus): void {
  if (this.updatingId()) return;

  this.updatingId.set(order.id);

  this.orderSvc.updateStatus(order, status).subscribe({
    next: (updatedOrder) => {   // ✅ CAPTURE RESPONSE
      this.orders.update(list =>
        list.map(o =>
          o.id === updatedOrder.id ? updatedOrder : o   // ✅ FULL REPLACE
        )
      );

      this.updatingId.set(null);
    },

    error: err => {
      console.error('Status update failed:', err);
      this.updatingId.set(null);
    }
  });
}

// ✅ UPDATED: Accept Date parameter
timeAgo(dateStr: Date | string): string {
  const date = typeof dateStr === 'string' ? new Date(dateStr) : dateStr;
  const mins = Math.floor((Date.now() - date.getTime()) / 60_000);
  if (mins < 1) return 'Just now';
  if (mins < 60) return `${mins}m ago`;
  return `${Math.floor(mins / 60)}h ago`;
}
  isUpdating(id: string): boolean {
    return this.updatingId() === id;
  }

  getStatusLabel(status: OrderStatus): string {
    return this.statusLabels[status] || 'Unknown';
  }

  // ✅ NEW: Get badge color based on status ENUM
  getStatusColor(status: OrderStatus): string {
    const colors: Record<OrderStatus, string> = {
      [OrderStatus.Pending]: 'badge-warning',
      [OrderStatus.Confirmed]: 'badge-info',
      [OrderStatus.Preparing]: 'badge-primary',
      [OrderStatus.Ready]: 'badge-secondary',
      [OrderStatus.PickedUp]: 'badge-secondary',
      [OrderStatus.OutForDelivery]: 'badge-secondary',
      [OrderStatus.Delivered]: 'badge-success',
      [OrderStatus.Cancelled]: 'badge-danger',
      [OrderStatus.Refunded]: 'badge-danger',
    };
    return colors[status] || 'badge-secondary';
  }
}