import { Component, OnInit, OnDestroy, signal, computed } from '@angular/core';
import { CommonModule }   from '@angular/common';
import { AuthService }    from '../../../core/auth/auth.service';
import { OrderService }   from '../../../core/services/order.service';

export type OrderStatus = 1 | 2 | 3 | 4 | 5;

export interface OrderItem {
  name:       string;
  quantity:   number;
  unitPrice:  number;
  totalPrice: number;
}

export interface Order {
  id:                  string;
  customerName:        string;
  items:               OrderItem[];
  totalAmount:         number;
  status:              OrderStatus;
  placedAt:            string;
  deliveryAddress:     string;
  subTotal:            number;
  taxes:               number;
  deliveryFee:         number;
  restaurantName:      string;
  paymentMethod:       string;
  estimatedDeliveryAt: string;
}

@Component({
  selector: 'app-restaurant-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './restaurant-dashboard.component.html',
  styleUrls: ['./restaurant-dashboard.component.scss'],
})
export class RestaurantDashboardComponent implements OnInit, OnDestroy {

  orders       = signal<Order[]>([]);
  loading      = signal(true);
  error        = signal<string | null>(null);
  updatingId   = signal<string | null>(null);
  activeFilter = signal<number | 'All'>(1);
  private pollInterval: any;

  filteredOrders = computed(() => {
    const f = this.activeFilter();
    return f === 'All'
      ? this.orders()
      : this.orders().filter(o => o.status === f);
  });

  pendingCount = computed(() =>
    this.orders().filter(o => o.status === 1).length);

  activeCount = computed(() =>
    this.orders().filter(o => o.status === 2 || o.status === 3).length);

  filters: Array<{ label: string; value: number | 'All' }> = [
    { label: 'All',       value: 'All'  },
    { label: 'Pending',   value: 1      },
    { label: 'Confirmed', value: 2      },
    { label: 'Preparing', value: 3      },
    { label: 'Ready',     value: 4      },
    { label: 'Cancelled', value: 5      },
  ];

  statusLabels: Record<OrderStatus, string> = {
    1: 'Pending',
    2: 'Confirmed',
    3: 'Preparing',
    4: 'Ready for Pickup',
    5: 'Cancelled',
  };

  constructor(
    private auth:     AuthService,
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

        const sorted = [...orders].sort((a, b) => {
          if (a.status === 1 && b.status !== 1) return -1;
          if (b.status === 1 && a.status !== 1) return  1;
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

  setFilter(f: number | 'All'): void {
    this.activeFilter.set(f);
  }

  acceptOrder(order: Order): void   { this.changeStatus(order, 2);   }
  rejectOrder(order: Order): void   { this.changeStatus(order, 5);   }
  markPreparing(order: Order): void { this.changeStatus(order, 3);   }
  markReady(order: Order): void     { this.changeStatus(order, 4);   }

  private changeStatus(order: Order, status: OrderStatus): void {
    if (this.updatingId()) return;
    this.updatingId.set(order.id);

    this.orderSvc.updateStatus(order.id, status).subscribe({
      next: () => {
        this.orders.update(list =>
          list.map(o => o.id === order.id ? { ...o, status } : o)
        );
        this.updatingId.set(null);
      },
      error: err => {
        console.error('Status update failed:', err);
        this.updatingId.set(null);
      },
    });
  }

  timeAgo(dateStr: string): string {
    const mins = Math.floor((Date.now() - new Date(dateStr).getTime()) / 60_000);
    if (mins < 1)  return 'Just now';
    if (mins < 60) return `${mins}m ago`;
    return `${Math.floor(mins / 60)}h ago`;
  }

  isUpdating(id: string): boolean {
    return this.updatingId() === id;
  }

  getStatusLabel(status: OrderStatus): string {
    return this.statusLabels[status];
  }
}