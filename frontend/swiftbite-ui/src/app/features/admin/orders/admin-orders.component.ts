import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { OrderService, OrderStatus } from '../../../core/services/order.service';
import { RestaurantService } from '../../../core/services/restaurant.service';

@Component({
  selector: 'app-admin-orders',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-orders.component.html',
  styleUrls: ['./admin-orders.component.scss'],
})
export class AdminOrdersComponent implements OnInit {

  private orderSvc = inject(OrderService);
  private restSvc  = inject(RestaurantService);

  loading    = signal(true);
  searchTerm = signal('');
  filterStatus = signal<string>('all');

  allOrders      = signal<any[]>([]);
  restaurants    = signal<any[]>([]);

  // Collect all orders from all restaurants
  filtered = computed(() => {
    let list = this.allOrders();
    const q  = this.searchTerm().toLowerCase();

    if (q) list = list.filter(o =>
      o.customerName?.toLowerCase().includes(q) ||
      o.restaurantName?.toLowerCase().includes(q) ||
      o.id?.toLowerCase().includes(q)
    );

    if (this.filterStatus() !== 'all') {
      const statusNum = parseInt(this.filterStatus());
      list = list.filter(o => o.status === statusNum);
    }

    return list.sort((a, b) =>
      new Date(b.placedAt).getTime() - new Date(a.placedAt).getTime()
    );
  });

  statusOptions = [
    { value: 'all', label: 'All' },
    { value: '1',   label: 'Pending' },
    { value: '2',   label: 'Confirmed' },
    { value: '3',   label: 'Preparing' },
    { value: '7',   label: 'Delivered' },
    { value: '8',   label: 'Cancelled' },
  ];

  ngOnInit(): void {
    // Load restaurants first, then orders per restaurant
    this.restSvc.getAll().subscribe({
      next: restaurants => {
        this.restaurants.set(restaurants);
        this.loadAllOrders(restaurants);
      },
      error: () => this.loading.set(false),
    });
  }

  private loadAllOrders(restaurants: any[]): void {
    if (restaurants.length === 0) { this.loading.set(false); return; }

    const allOrders: any[] = [];
    let completed = 0;

    restaurants.forEach(r => {
      this.orderSvc.getRestaurantOrders(r.id).subscribe({
        next: orders => {
          allOrders.push(...orders);
          completed++;
          if (completed === restaurants.length) {
            this.allOrders.set(allOrders);
            this.loading.set(false);
          }
        },
        error: () => {
          completed++;
          if (completed === restaurants.length) {
            this.allOrders.set(allOrders);
            this.loading.set(false);
          }
        },
      });
    });
  }

  statusLabel(status: number): string {
    return OrderStatus[status] ?? 'Unknown';
  }

  statusClass(status: number): string {
    const map: Record<number, string> = {
      1: 'badge-pending', 2: 'badge-confirmed', 3: 'badge-preparing',
      4: 'badge-ready',   5: 'badge-ready',     6: 'badge-delivery',
      7: 'badge-delivered', 8: 'badge-cancelled', 9: 'badge-cancelled',
    };
    return map[status] ?? 'badge-pending';
  }

  timeAgo(dateStr: string): string {
    const mins = Math.floor((Date.now() - new Date(dateStr).getTime()) / 60000);
    if (mins < 1)    return 'Just now';
    if (mins < 60)   return `${mins}m ago`;
    if (mins < 1440) return `${Math.floor(mins/60)}h ago`;
    return `${Math.floor(mins/1440)}d ago`;
  }
}