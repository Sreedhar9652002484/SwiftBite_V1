import { Component, OnInit, OnDestroy, signal, computed } from '@angular/core';
import { CommonModule }   from '@angular/common';
import { AuthService }    from '../../../core/auth/auth.service';
import { OrderService }   from '../../../core/services/order.service';

export type OrderStatus =
  | 'Pending'
  | 'Confirmed'
  | 'Preparing'
  | 'ReadyForPickup'
  | 'Cancelled';

export interface OrderItem {
  name:     string;
  quantity: number;
  price:    number;
}

export interface Order {
  id:           string;
  customerName: string;
  items:        OrderItem[];
  totalAmount:  number;
  status:       OrderStatus;
  createdAt:    string;
  address?:     string;
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
  activeFilter = signal<OrderStatus | 'All'>('All');
  private pollInterval: any;

  filteredOrders = computed(() => {
    const f = this.activeFilter();
    return f === 'All'
      ? this.orders()
      : this.orders().filter(o => o.status === f);
  });

  pendingCount = computed(() =>
    this.orders().filter(o => o.status === 'Pending').length);

  activeCount = computed(() =>
    this.orders().filter(o =>
      o.status === 'Confirmed' || o.status === 'Preparing').length);

  filters: Array<{ label: string; value: OrderStatus | 'All' }> = [
    { label: 'All',       value: 'All'            },
    { label: 'Pending',   value: 'Pending'         },
    { label: 'Confirmed', value: 'Confirmed'       },
    { label: 'Preparing', value: 'Preparing'       },
    { label: 'Ready',     value: 'ReadyForPickup'  },
    { label: 'Cancelled', value: 'Cancelled'       },
  ];

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
          if (a.status === 'Pending' && b.status !== 'Pending') return -1;
          if (b.status === 'Pending' && a.status !== 'Pending') return  1;
          return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
        });
        this.orders.set(sorted);
        this.loading.set(false);
        this.error.set(null);
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

  acceptOrder(order: Order): void   { this.changeStatus(order, 'Confirmed');      }
  rejectOrder(order: Order): void   { this.changeStatus(order, 'Cancelled');      }
  markPreparing(order: Order): void { this.changeStatus(order, 'Preparing');      }
  markReady(order: Order): void     { this.changeStatus(order, 'ReadyForPickup'); }

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
}