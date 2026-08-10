import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { RestaurantService, Restaurant, CuisineType } from '../../../core/services/restaurant.service';
import { NotificationBellComponent } from '../../shared/components/notification-bell/notification-bell.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink, NotificationBellComponent],
  templateUrl: './home.component.html'
})
export class HomeComponent implements OnInit {
  authService = inject(AuthService);
  router      = inject(Router);
  private restSvc = inject(RestaurantService);

  restaurants  = signal<Restaurant[]>([]);
  loading      = signal(true);
  error        = signal('');
  searchQuery  = signal('');
  selectedCity = signal('Hyderabad');

  categories = [
    { emoji: '🍕', name: 'Pizza',    cuisine: CuisineType.Pizza    },
    { emoji: '🍔', name: 'Burgers',  cuisine: CuisineType.Burgers  },
    { emoji: '🍜', name: 'Chinese',  cuisine: CuisineType.Chinese  },
    { emoji: '🍱', name: 'Biryani',  cuisine: CuisineType.Biryani  },
    { emoji: '🥗', name: 'Healthy',  cuisine: CuisineType.Healthy  },
    { emoji: '🌮', name: 'Mexican',  cuisine: CuisineType.Mexican  },
    { emoji: '🍦', name: 'Desserts', cuisine: CuisineType.Desserts },
    { emoji: '☕', name: 'Cafe',     cuisine: CuisineType.Cafe     },
    { emoji: '🍣', name: 'Japanese', cuisine: CuisineType.Japanese },
  ];

  cuisineEmoji: Record<number, string> = {
    [CuisineType.Pizza]:         '🍕',
    [CuisineType.Burgers]:       '🍔',
    [CuisineType.Chinese]:       '🍜',
    [CuisineType.Biryani]:       '🍱',
    [CuisineType.Indian]:        '🫕',
    [CuisineType.Mexican]:       '🌮',
    [CuisineType.Desserts]:      '🍦',
    [CuisineType.Cafe]:          '☕',
    [CuisineType.Japanese]:      '🍣',
    [CuisineType.Italian]:       '🍝',
    [CuisineType.FastFood]:      '🍟',
    [CuisineType.Healthy]:       '🥗',
    [CuisineType.NorthIndian]:   '🫕',
    [CuisineType.SouthIndian]:   '🫕',
    [CuisineType.Seafood]:       '🦐',
    [CuisineType.Vegan]:         '🥗',
    [CuisineType.Bakery]:        '🥐',
    [CuisineType.Continental]:   '🍽️',
    [CuisineType.Vietnamese]:    '🍲',
    [CuisineType.Korean]:        '🍜',
    [CuisineType.Thai]:          '🍲',
    [CuisineType.Mediterranean]: '🫒',
    [CuisineType.MiddleEastern]: '🫕',
    [CuisineType.Turkish]:       '🌯',
    [CuisineType.Spanish]:       '🥘',
    [CuisineType.Greek]:         '🫒',
    [CuisineType.Portuguese]:    '🍤',
    [CuisineType.Lebanese]:      '🌯',
    [CuisineType.Pakistani]:     '🫕',
    [CuisineType.Afghan]:        '🍖',
    [CuisineType.American]:      '🍔',
    [CuisineType.Beverages]:     '☕',
    [CuisineType.MultiCuisine]:  '🍽️',
  };

  ngOnInit(): void {
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/auth/login']);
      return;
    }
    this.loadRestaurants();
  }

  loadRestaurants(): void {
    this.loading.set(true);
    this.error.set('');
    this.restSvc.getAll().subscribe({
      next: data => {
        this.restaurants.set(data);
        this.loading.set(false);
      },
      error: err => {
        console.error('API error:', err);
        this.restaurants.set(this.getMockData());
        this.loading.set(false);
        this.error.set('Using demo data — API not connected yet');
      }
    });
  }

  filterByCuisine(cuisineEnum: CuisineType): void {
    const filtered = this.restaurants().filter(r =>
      // compare as numbers since cuisineType may be number | string
      Number(r.cuisineType) === Number(cuisineEnum)
    );
    if (filtered.length > 0) {
      this.restaurants.set(filtered);
    } else {
      this.loadRestaurants();
    }
  }

  onSearch(query: string): void {
    this.searchQuery.set(query);
    if (!query.trim()) {
      this.loadRestaurants();
      return;
    }
    this.restSvc.searchItems(query).subscribe({
      next: () => {},
      error: () => {}
    });
  }

  goToRestaurant(id: string): void {
    this.router.navigate(['/restaurant', id]);
  }

  /**
   * Accepts number | string | CuisineType — normalises to number key
   * so the Record<number, string> lookup always works.
   */
  getEmoji(cuisineType: CuisineType | string | number): string {
    return this.cuisineEmoji[Number(cuisineType)] ?? '🍽️';
  }

  getRatingColor(rating: number): string {
    if (rating >= 4.5) return '#22c55e';
    if (rating >= 4.0) return '#f59e0b';
    return '#ef4444';
  }

  getCuisineName(cuisineType: CuisineType | string | number): string {
    const key = Number(cuisineType);
    return CuisineType[key] ?? String(cuisineType);
  }

  getMockData(): Restaurant[] {
    return [
      {
        id: '1', ownerId: 'owner-1',
        name: 'Paradise Biryani', description: 'Best Biryani in Hyderabad',
        phoneNumber: '9999999999', email: 'paradise@biryani.com',
        address: 'Secunderabad', city: 'Hyderabad', pinCode: '500003',
        latitude: 17.3845, longitude: 78.4867,
        cuisineType: CuisineType.Biryani,
        minimumOrderAmount: 150, averageDeliveryTimeMinutes: 35,
        isOpen: true, rating: 4.5, reviewCount: 2400,
        averageRating: 4.5, totalRatings: 2400, status: 1,
        createdAt: new Date(),
        bannerUrl: 'https://via.placeholder.com/400x200?text=Paradise+Biryani',
        logoUrl:   'https://via.placeholder.com/100x100?text=PB',
      },
      {
        id: '2', ownerId: 'owner-2',
        name: 'Pizza Hut', description: 'Pizzas and more',
        phoneNumber: '9999999998', email: 'pizza@hut.com',
        address: 'Banjara Hills', city: 'Hyderabad', pinCode: '500034',
        latitude: 17.3850, longitude: 78.4890,
        cuisineType: CuisineType.Pizza,
        minimumOrderAmount: 200, averageDeliveryTimeMinutes: 30,
        isOpen: true, rating: 4.2, reviewCount: 1800,
        averageRating: 4.2, totalRatings: 1800, status: 1,
        createdAt: new Date(),
        bannerUrl: 'https://via.placeholder.com/400x200?text=Pizza+Hut',
        logoUrl:   'https://via.placeholder.com/100x100?text=PH',
      },
      {
        id: '3', ownerId: 'owner-3',
        name: "McDonald's", description: 'Fast Food',
        phoneNumber: '9999999997', email: 'mcdonalds@fastfood.com',
        address: 'Jubilee Hills', city: 'Hyderabad', pinCode: '500033',
        latitude: 17.3900, longitude: 78.5000,
        cuisineType: CuisineType.Burgers,
        minimumOrderAmount: 100, averageDeliveryTimeMinutes: 25,
        isOpen: true, rating: 4.3, reviewCount: 3200,
        averageRating: 4.3, totalRatings: 3200, status: 1,
        createdAt: new Date(),
        bannerUrl: 'https://via.placeholder.com/400x200?text=McDonalds',
        logoUrl:   'https://via.placeholder.com/100x100?text=MC',
      },
      {
        id: '4', ownerId: 'owner-4',
        name: 'Behrouz Biryani', description: 'Royal Biryani Experience',
        phoneNumber: '9999999996', email: 'behrouz@biryani.com',
        address: 'Madhapur', city: 'Hyderabad', pinCode: '500081',
        latitude: 17.4500, longitude: 78.5500,
        cuisineType: CuisineType.Biryani,
        minimumOrderAmount: 300, averageDeliveryTimeMinutes: 45,
        isOpen: true, rating: 4.6, reviewCount: 980,
        averageRating: 4.6, totalRatings: 980, status: 1,
        createdAt: new Date(),
        bannerUrl: 'https://via.placeholder.com/400x200?text=Behrouz',
        logoUrl:   'https://via.placeholder.com/100x100?text=BB',
      },
      {
        id: '5', ownerId: 'owner-5',
        name: 'Subway', description: 'Fresh sandwiches and salads',
        phoneNumber: '9999999995', email: 'subway@healthy.com',
        address: 'Gachibowli', city: 'Hyderabad', pinCode: '500032',
        latitude: 17.4400, longitude: 78.4600,
        cuisineType: CuisineType.Healthy,
        minimumOrderAmount: 150, averageDeliveryTimeMinutes: 25,
        isOpen: false, rating: 4.1, reviewCount: 760,
        averageRating: 4.1, totalRatings: 760, status: 1,
        createdAt: new Date(),
        bannerUrl: 'https://via.placeholder.com/400x200?text=Subway',
        logoUrl:   'https://via.placeholder.com/100x100?text=SW',
      },
      {
        id: '6', ownerId: 'owner-6',
        name: "Domino's Pizza", description: 'Pizza and more',
        phoneNumber: '9999999994', email: 'dominos@pizza.com',
        address: 'Kondapur', city: 'Hyderabad', pinCode: '500081',
        latitude: 17.4700, longitude: 78.5300,
        cuisineType: CuisineType.Pizza,
        minimumOrderAmount: 199, averageDeliveryTimeMinutes: 35,
        isOpen: true, rating: 4.4, reviewCount: 2100,
        averageRating: 4.4, totalRatings: 2100, status: 1,
        createdAt: new Date(),
        bannerUrl: 'https://via.placeholder.com/400x200?text=Dominos',
        logoUrl:   'https://via.placeholder.com/100x100?text=DO',
      },
    ];
  }
}