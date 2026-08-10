import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RestaurantService } from '../../../core/services/restaurant.service';

@Component({
  selector: 'app-admin-restaurants',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-restaurants.component.html',
  styleUrls: ['./admin-restaurants.component.scss'],
})
export class AdminRestaurantsComponent implements OnInit {

  private restSvc = inject(RestaurantService);

  loading    = signal(true);
  saving     = signal<string | null>(null);
  errorMsg   = signal<string | null>(null);
  successMsg = signal<string | null>(null);
  searchTerm = signal('');
  filterOpen = signal<'all' | 'open' | 'closed'>('all');

  restaurants = signal<any[]>([]);

  filtered = computed(() => {
    let list = this.restaurants();
    const q  = this.searchTerm().toLowerCase();

    if (q) list = list.filter(r =>
      r.name?.toLowerCase().includes(q) ||
      r.city?.toLowerCase().includes(q)
    );

    if (this.filterOpen() === 'open')   list = list.filter(r => r.isOpen);
    if (this.filterOpen() === 'closed') list = list.filter(r => !r.isOpen);

    return list;
  });

  ngOnInit(): void {
    this.restSvc.getAll().subscribe({
      next:  r => { this.restaurants.set(r); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  toggleStatus(restaurant: any): void {
    if (this.saving() === restaurant.id) return;
    this.saving.set(restaurant.id);

    this.restSvc.toggleRestaurantStatus(restaurant.id).subscribe({
      next: res => {
        this.restaurants.update(list =>
          list.map(r => r.id === restaurant.id ? { ...r, isOpen: res.isOpen } : r)
        );
        this.saving.set(null);
        this.toast('success', `${restaurant.name} is now ${res.isOpen ? 'Open' : 'Closed'}`);
      },
      error: () => { this.saving.set(null); this.toast('error', 'Failed to toggle status.'); },
    });
  }

  cuisineLabel(type: number): string {
    const map: Record<number, string> = {
      1:'Indian', 2:'Chinese', 3:'Italian', 4:'Mexican', 5:'American',
      6:'Thai', 7:'Japanese', 8:'Mediterranean', 9:'Fast Food', 10:'Desserts',
      11:'Beverages', 12:'Multi Cuisine', 13:'Biryani', 14:'North Indian',
      15:'South Indian', 16:'Continental', 31:'Pizza', 32:'Burgers',
      33:'Healthy', 30:'Cafe',
    };
    return map[type] ?? `Type ${type}`;
  }

  private toast(type: 'success' | 'error', msg: string): void {
    if (type === 'success') { this.successMsg.set(msg); setTimeout(() => this.successMsg.set(null), 3000); }
    else                    { this.errorMsg.set(msg);   setTimeout(() => this.errorMsg.set(null),   4000); }
  }
}