import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule }  from '@angular/common';
import { AuthService }   from '../../../core/auth/auth.service';
import { OrderService }  from '../../../core/services/order.service';

type Period = '7d' | '30d' | '90d';

interface DailyRevenue {
  date:     string;   // 'Mon', 'Tue' etc
  revenue:  number;
  orders:   number;
}

interface TopItem {
  name:     string;
  count:    number;
  revenue:  number;
}

@Component({
  selector: 'app-analytics',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './analytics.component.html',
  styleUrls:   ['./analytics.component.scss'],
})
export class AnalyticsComponent implements OnInit {

  private auth     = inject(AuthService);
  private orderSvc = inject(OrderService);

  loading   = signal(true);
  errorMsg  = signal<string | null>(null);
  period    = signal<Period>('7d');

  allOrders = signal<any[]>([]);

  // ── Filtered by period ────────────────────────────────────
  filteredOrders = computed(() => {
    const days = this.period() === '7d' ? 7 : this.period() === '30d' ? 30 : 90;
    const cutoff = new Date();
    cutoff.setDate(cutoff.getDate() - days);
    return this.allOrders().filter(o =>
      new Date(o.createdAt) >= cutoff
    );
  });

  // ── Summary stats ─────────────────────────────────────────
  totalRevenue = computed(() =>
    this.deliveredOrders().reduce((s, o) => s + (o.totalAmount ?? 0), 0)
  );

  totalOrders = computed(() => this.filteredOrders().length);

  deliveredOrders = computed(() =>
    this.filteredOrders().filter(o => o.status === 'Delivered')
  );

  cancelledOrders = computed(() =>
    this.filteredOrders().filter(o => o.status === 'Cancelled')
  );

  avgOrderValue = computed(() => {
    const d = this.deliveredOrders();
    if (!d.length) return 0;
    return d.reduce((s, o) => s + (o.totalAmount ?? 0), 0) / d.length;
  });

  completionRate = computed(() => {
    const total = this.filteredOrders().length;
    if (!total) return 0;
    return Math.round((this.deliveredOrders().length / total) * 100);
  });

  // ── Daily revenue for bar chart ───────────────────────────
  dailyData = computed((): DailyRevenue[] => {
    const days  = this.period() === '7d' ? 7 : this.period() === '30d' ? 30 : 90;
    const slots = this.period() === '7d' ? 7 : this.period() === '30d' ? 10 : 9;
    const bucketSize = Math.ceil(days / slots);
    const result: DailyRevenue[] = [];

    for (let i = slots - 1; i >= 0; i--) {
      const end   = new Date(); end.setDate(end.getDate() - i * bucketSize);
      const start = new Date(); start.setDate(start.getDate() - (i + 1) * bucketSize);

      const label = this.period() === '7d'
        ? end.toLocaleDateString('en', { weekday: 'short' })
        : end.toLocaleDateString('en', { day: 'numeric', month: 'short' });

      const bucket = this.deliveredOrders().filter(o => {
        const d = new Date(o.createdAt);
        return d >= start && d < end;
      });

      result.push({
        date:    label,
        revenue: bucket.reduce((s, o) => s + (o.totalAmount ?? 0), 0),
        orders:  bucket.length,
      });
    }
    return result;
  });

  maxRevenue = computed(() =>
    Math.max(...this.dailyData().map(d => d.revenue), 1)
  );

  // ── Top selling items ─────────────────────────────────────
  topItems = computed((): TopItem[] => {
    const map = new Map<string, TopItem>();

    this.deliveredOrders().forEach(order => {
      (order.items ?? []).forEach((item: any) => {
        const name = item.name ?? item.menuItemName ?? 'Unknown';
        const qty  = item.quantity ?? 1;
        const rev  = (item.price ?? 0) * qty;
        const existing = map.get(name);
        if (existing) {
          existing.count   += qty;
          existing.revenue += rev;
        } else {
          map.set(name, { name, count: qty, revenue: rev });
        }
      });
    });

    return Array.from(map.values())
      .sort((a, b) => b.count - a.count)
      .slice(0, 5);
  });

  // ── Order status breakdown ────────────────────────────────
  statusBreakdown = computed(() => {
    const statuses = ['Pending','Confirmed','Preparing','ReadyForPickup','OutForDelivery','Delivered','Cancelled'];
    return statuses.map(s => ({
      status: s,
      count:  this.filteredOrders().filter(o => o.status === s).length,
    })).filter(s => s.count > 0);
  });

  private get rid(): string | null {
    const u = this.auth.currentUser();
    return u?.['restaurantId'] ?? u?.['restaurant_id'] ?? null;
  }

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    const rid = this.rid;
    if (!rid) { this.loading.set(false); return; }

    this.loading.set(true);
    this.orderSvc.getRestaurantOrders(rid).subscribe({
      next:  orders => { this.allOrders.set(orders); this.loading.set(false); },
      error: ()     => { this.loading.set(false); this.errorMsg.set('Failed to load analytics data.'); },
    });
  }

  setPeriod(p: Period): void { this.period.set(p); }

  barHeight(revenue: number): number {
    return Math.round((revenue / this.maxRevenue()) * 100);
  }

  statusClass(status: string): string {
    const map: Record<string, string> = {
      Delivered: 'dot-delivered', Cancelled: 'dot-cancelled',
      Pending: 'dot-pending', Confirmed: 'dot-confirmed',
      Preparing: 'dot-preparing', ReadyForPickup: 'dot-ready',
      OutForDelivery: 'dot-delivery',
    };
    return map[status] ?? 'dot-pending';
  }
}