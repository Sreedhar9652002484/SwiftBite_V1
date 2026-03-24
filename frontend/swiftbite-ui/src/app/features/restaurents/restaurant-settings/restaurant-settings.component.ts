import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule }      from '@angular/common';
import { FormsModule }       from '@angular/forms';
import { AuthService }       from '../../../core/auth/auth.service';
import { RestaurantService } from '../../../core/services/restaurant.service';

// Matches CuisineType enum — integer value sent to backend
const CUISINE_TYPES: { label: string; value: number }[] = [
  { label: 'Indian',        value: 1  },
  { label: 'Italian',       value: 2  },
  { label: 'Chinese',       value: 3  },
  { label: 'Mexican',       value: 4  },
  { label: 'Japanese',      value: 5  },
  { label: 'American',      value: 6  },
  { label: 'Mediterranean', value: 7  },
  { label: 'Thai',          value: 8  },
  { label: 'French',        value: 9  },
  { label: 'Spanish',       value: 10 },
  { label: 'Biryani',       value: 11 },
  { label: 'Pizza',         value: 12 },
  { label: 'Burgers',       value: 13 },
  { label: 'Healthy',       value: 14 },
  { label: 'Fast Food',     value: 15 },
  { label: 'Desserts',      value: 16 },
  { label: 'Cafe',          value: 17 },
  { label: 'Sushi',         value: 18 },
  { label: 'Kebabs',        value: 19 },
  { label: 'Mughlai',       value: 20 },
  { label: 'South Indian',  value: 21 },
  { label: 'North Indian',  value: 22 },
  { label: 'Seafood',       value: 23 },
  { label: 'Vegan',         value: 24 },
];

@Component({
  selector: 'app-restaurant-settings',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './restaurant-settings.component.html',
  styleUrls:   ['./restaurant-settings.component.scss'],
})
export class RestaurantSettingsComponent implements OnInit {

  private auth    = inject(AuthService);
  private restSvc = inject(RestaurantService);

  cuisineTypes = CUISINE_TYPES;

  loading    = signal(true);
  saving     = signal(false);
  successMsg = signal<string | null>(null);
  errorMsg   = signal<string | null>(null);

  // cuisineType stored as number matching enum integer value
  form = {
    name:                       '',
    description:                '',
    phoneNumber:                '',
    address:                    '',
    city:                       '',
    pinCode:                    '',
    minimumOrderAmount:         0,
    averageDeliveryTimeMinutes: 30,
    cuisineType:                1,   // integer — default: Indian
  };

  private get rid(): string | null {
    const u = this.auth.currentUser();
    return u?.['restaurantId'] ?? u?.['restaurant_id'] ?? null;
  }

  ngOnInit(): void {
    this.loadRestaurant();
  }

  private loadRestaurant(): void {
    const rid = this.rid;
    if (!rid) { this.loading.set(false); return; }

    this.restSvc.getById(rid).subscribe({
      next: (r: any) => {
        this.form = {
          name:                       r.name                         ?? '',
          description:                r.description                  ?? '',
          phoneNumber:                r.phoneNumber                  ?? '',
          address:                    r.address                      ?? '',
          city:                       r.city                         ?? '',
          pinCode:                    r.pinCode                      ?? '',
          minimumOrderAmount:         r.minimumOrderAmount           ?? 0,
          averageDeliveryTimeMinutes: r.averageDeliveryTimeMinutes   ?? 30,
          cuisineType:                r.cuisineType                  ?? 1,
        };
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.toast('error', 'Failed to load restaurant details.');
      },
    });
  }

  save(): void {
    const rid = this.rid;
    if (!rid) return;
    if (!this.form.name.trim()) { this.toast('error', 'Restaurant name is required.'); return; }

    this.saving.set(true);
    this.restSvc.updateRestaurant(rid, {
      ...this.form,
      cuisineType: Number(this.form.cuisineType),  // ensure int, not string from select
    }).subscribe({
      next: () => { this.saving.set(false); this.toast('success', 'Settings saved!'); },
      error: () => { this.saving.set(false); this.toast('error', 'Failed to save. Try again.'); },
    });
  }

  private toast(type: 'success' | 'error', msg: string): void {
    if (type === 'success') { this.successMsg.set(msg); setTimeout(() => this.successMsg.set(null), 3000); }
    else                    { this.errorMsg.set(msg);   setTimeout(() => this.errorMsg.set(null),   4000); }
  }
}