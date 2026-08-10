import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { RestaurantService } from '../../../core/services/restaurant.service';
import { OrderService, OrderStatus } from '../../../core/services/order.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.scss'],
})
export class AdminDashboardComponent implements OnInit {

  private restSvc  = inject(RestaurantService);
  private orderSvc = inject(OrderService);

  loadingRestaurants = signal(true);
  loadingOrders      = signal(true);

  restaurants = signal<any[]>([]);
  orders      = signal<any[]>([]);

  // ── Stats ──────────────────────────────────────────────────
  totalRestaurants  = computed(() => this.restaurants().length);
  openRestaurants   = computed(() => this.restaurants().filter(r => r.isOpen).length);

  totalOrders       = computed(() => this.orders().length);
  totalRevenue      = computed(() =>
    this.orders()
      .filter(o => o.status === OrderStatus.Delivered)
      .reduce((s, o) => s + (o.totalAmount ?? 0), 0)
  );
  pendingOrders     = computed(() =>
    this.orders().filter(o => o.status === OrderStatus.Pending).length
  );
  deliveredOrders   = computed(() =>
    this.orders().filter(o => o.status === OrderStatus.Delivered).length
  );
  cancelledOrders   = computed(() =>
    this.orders().filter(o => o.status === OrderStatus.Cancelled).length
  );

  // ── Recent orders (last 10) ────────────────────────────────
  recentOrders = computed(() =>
    [...this.orders()]
      .sort((a, b) => new Date(b.placedAt).getTime() - new Date(a.placedAt).getTime())
      .slice(0, 10)
  );

  // ── Top restaurants by order count ────────────────────────
  topRestaurants = computed(() => {
    const counts = new Map<string, { name: string; count: number; revenue: number }>();
    this.orders().forEach(o => {
      const existing = counts.get(o.restaurantId);
      if (existing) {
        existing.count++;
        existing.revenue += o.totalAmount ?? 0;
      } else {
        counts.set(o.restaurantId, { name: o.restaurantName ?? 'Unknown', count: 1, revenue: o.totalAmount ?? 0 });
      }
    });
    return Array.from(counts.values())
      .sort((a, b) => b.count - a.count)
      .slice(0, 5);
  });

  ngOnInit(): void {
    this.loadRestaurants();
    this.loadOrders();
  }

  private loadRestaurants(): void {
    this.restSvc.getAll().subscribe({
      next:  r => { this.restaurants.set(r); this.loadingRestaurants.set(false); },
      error: () => this.loadingRestaurants.set(false),
    });
  }

  private loadOrders(): void {
    // Admin sees all orders — we aggregate across all restaurants
    // Since there's no GET /api/orders/all yet, we collect from all restaurants
    // TODO: Add GET /api/orders/all endpoint to OrderService backend
    this.loadingOrders.set(false);
  }

  statusLabel(status: number): string {
    return OrderStatus[status] ?? 'Unknown';
  }

  statusClass(status: number): string {
    const map: Record<number, string> = {
      [OrderStatus.Pending]:        'badge-pending',
      [OrderStatus.Confirmed]:      'badge-confirmed',
      [OrderStatus.Preparing]:      'badge-preparing',
      [OrderStatus.Ready]:          'badge-ready',
      [OrderStatus.OutForDelivery]: 'badge-delivery',
      [OrderStatus.Delivered]:      'badge-delivered',
      [OrderStatus.Cancelled]:      'badge-cancelled',
    };
    return map[status] ?? 'badge-pending';
  }

  timeAgo(dateStr: string): string {
    const mins = Math.floor((Date.now() - new Date(dateStr).getTime()) / 60000);
    if (mins < 1)  return 'Just now';
    if (mins < 60) return `${mins}m ago`;
    if (mins < 1440) return `${Math.floor(mins / 60)}h ago`;
    return `${Math.floor(mins / 1440)}d ago`;
  }

  get loading(): boolean {
    return this.loadingRestaurants();
  }
}