import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RestaurantService } from '../../../core/services/restaurant.service';
import { OrderService, OrderStatus } from '../../../core/services/order.service';

type Period = '7d' | '30d' | '90d';

@Component({
  selector: 'app-admin-analytics',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './admin-analytics.component.html',
  styleUrls: ['./admin-analytics.component.scss'],
})
export class AdminAnalyticsComponent implements OnInit {

  private restSvc  = inject(RestaurantService);
  private orderSvc = inject(OrderService);

  loading      = signal(true);
  error        = signal<string | null>(null);
  period       = signal<Period>('7d');
  allOrders    = signal<any[]>([]);
  restaurants  = signal<any[]>([]);

  // ── Filter by period ───────────────────────────────────────
  filtered = computed(() => {
    const days = this.period() === '7d' ? 7 : this.period() === '30d' ? 30 : 90;
    const cutoff = new Date();
    cutoff.setDate(cutoff.getDate() - days);
    return this.allOrders().filter(o => new Date(o.placedAt) >= cutoff);
  });

  delivered = computed(() => this.filtered().filter(o => o.status === OrderStatus.Delivered));

  totalRevenue    = computed(() => this.delivered().reduce((s, o) => s + (o.totalAmount ?? 0), 0));
  totalOrders     = computed(() => this.filtered().length);
  avgOrderValue   = computed(() => this.delivered().length ? this.totalRevenue() / this.delivered().length : 0);
  completionRate  = computed(() => this.totalOrders() ? Math.round((this.delivered().length / this.totalOrders()) * 100) : 0);

  maxRevenue = computed(() => Math.max(...this.dailyData().map(d => d.revenue), 1));

  dailyData = computed(() => {
    const days    = this.period() === '7d' ? 7 : this.period() === '30d' ? 30 : 90;
    const slots   = this.period() === '7d' ? 7 : this.period() === '30d' ? 10 : 9;
    const bucket  = Math.ceil(days / slots);
    const result: {date?: string; label: string; revenue: number; orders: number }[] = [];

    for (let i = slots - 1; i >= 0; i--) {
      const end   = new Date(); end.setDate(end.getDate() - i * bucket);
      const start = new Date(); start.setDate(start.getDate() - (i + 1) * bucket);
      const label = this.period() === '7d'
        ? end.toLocaleDateString('en', { weekday: 'short' })
        : end.toLocaleDateString('en', { day: 'numeric', month: 'short' });

      const slice = this.delivered().filter(o => {
        const d = new Date(o.placedAt);
        return d >= start && d < end;
      });
      result.push({ label, revenue: slice.reduce((s, o) => s + (o.totalAmount ?? 0), 0), orders: slice.length });
    }
    return result;
  });

  topRestaurants = computed(() => {
    const map = new Map<string, { name: string; orders: number; revenue: number }>();
    this.delivered().forEach(o => {
      const e = map.get(o.restaurantId);
      if (e) { e.orders++; e.revenue += o.totalAmount ?? 0; }
      else map.set(o.restaurantId, { name: o.restaurantName ?? 'Unknown', orders: 1, revenue: o.totalAmount ?? 0 });
    });
    return Array.from(map.values()).sort((a, b) => b.revenue - a.revenue).slice(0, 5);
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.error.set(null);
    this.loading.set(true);
    this.restSvc.getAllForAdmin().subscribe({
      next: restaurants => {
        this.restaurants.set(restaurants);
        this.loadAllOrders();
      },
      error: err => {
        this.error.set('Failed to load restaurants. Please try again.');
        this.loading.set(false);
        console.error(err);
      },
    });
  }

  private loadAllOrders(): void {
    this.orderSvc.getAllOrdersAdmin().subscribe({
      next: orders => { this.allOrders.set(orders); this.loading.set(false); },
      error: err => {
        this.error.set('Failed to load orders. Please try again.');
        this.loading.set(false);
        console.error(err);
      },
    });
  }

  setPeriod(p: Period): void { this.period.set(p); }
  barHeight(revenue: number): number { return Math.round((revenue / this.maxRevenue()) * 100); }
}