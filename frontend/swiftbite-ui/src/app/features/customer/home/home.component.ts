import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { RestaurantService } from '../../../core/services/restaurant.service';
import { Restaurant } from '../../../core/models/restaurant.model';
import { NotificationBellComponent } from '../../shared/components/notification-bell/notification-bell.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink, NotificationBellComponent],
  templateUrl: './home.component.html'
})
export class HomeComponent implements OnInit {
  authService  = inject(AuthService);
  router       = inject(Router);
  private restSvc = inject(RestaurantService);

  // ✅ Real data from API
  restaurants  = signal<Restaurant[]>([]);
  loading      = signal(true);
  error        = signal('');
  searchQuery  = signal('');
  selectedCity = signal('Hyderabad');

  categories = [
    { emoji: '🍕', name: 'Pizza',    cuisine: 'Pizza' },
    { emoji: '🍔', name: 'Burgers',  cuisine: 'Burgers' },
    { emoji: '🍜', name: 'Chinese',  cuisine: 'Chinese' },
    { emoji: '🍱', name: 'Biryani',  cuisine: 'Biryani' },
    { emoji: '🥗', name: 'Healthy',  cuisine: 'Healthy' },
    { emoji: '🌮', name: 'Mexican',  cuisine: 'Mexican' },
    { emoji: '🍦', name: 'Desserts', cuisine: 'Desserts' },
    { emoji: '☕', name: 'Cafe',     cuisine: 'Cafe' },
    { emoji: '🍣', name: 'Sushi',    cuisine: 'Sushi' },
  ];

  // Emoji fallback when no image
  cuisineEmoji: Record<string, string> = {
    'Pizza': '🍕', 'Burgers': '🍔', 'Chinese': '🍜',
    'Biryani': '🍱', 'Indian': '🫕', 'Mexican': '🌮',
    'Desserts': '🍦', 'Cafe': '☕', 'Sushi': '🍣',
    'Italian': '🍝', 'FastFood': '🍟', 'Healthy': '🥗',
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

    // this.restSvc.getByCity(this.selectedCity())
    this.restSvc.getAll()
      .subscribe({
        next: data => {
          this.restaurants.set(data);
          this.loading.set(false);
        },
        error: err => {
          console.error('API error:', err);
          // ✅ Fallback to mock data if API not ready
          this.restaurants.set(this.getMockData());
          this.loading.set(false);
          this.error.set(
            'Using demo data — API not connected yet');
        }
      });
  }

  filterByCuisine(cuisine: string): void {
    const all = this.restaurants();
    const filtered = all.filter(r =>
      r.cuisineType?.toLowerCase()
        .includes(cuisine.toLowerCase()));
    if (filtered.length > 0)
      this.restaurants.set(filtered);
    else
      this.loadRestaurants(); // Reset if no match
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

  getEmoji(cuisineType: string): string {
    return this.cuisineEmoji[cuisineType] ?? '🍽️';
  }

  getRatingColor(rating: number): string {
    if (rating >= 4.5) return '#22c55e';
    if (rating >= 4.0) return '#f59e0b';
    return '#ef4444';
  }

  // ✅ Mock fallback when backend not running
  getMockData(): Restaurant[] {
    return [
      { id: '1', name: 'Paradise Biryani',
        cuisineType: 'Biryani',
        city: 'Hyderabad', address: 'Secunderabad',
        averageRating: 4.5, totalRatings: 2400,
        averageDeliveryTimeMinutes: 35,
        minimumOrderAmount: 150,
        isOpen: true, status: 'Active',
        description: 'Best Biryani in Hyderabad' },
      { id: '2', name: 'Pizza Hut',
        cuisineType: 'Pizza',
        city: 'Hyderabad', address: 'Banjara Hills',
        averageRating: 4.2, totalRatings: 1800,
        averageDeliveryTimeMinutes: 30,
        minimumOrderAmount: 200,
        isOpen: true, status: 'Active',
        description: 'Pizzas and more' },
      { id: '3', name: "McDonald's",
        cuisineType: 'Burgers',
        city: 'Hyderabad', address: 'Jubilee Hills',
        averageRating: 4.3, totalRatings: 3200,
        averageDeliveryTimeMinutes: 25,
        minimumOrderAmount: 100,
        isOpen: true, status: 'Active',
        description: 'Fast Food' },
      { id: '4', name: 'Behrouz Biryani',
        cuisineType: 'Biryani',
        city: 'Hyderabad', address: 'Madhapur',
        averageRating: 4.6, totalRatings: 980,
        averageDeliveryTimeMinutes: 45,
        minimumOrderAmount: 300,
        isOpen: true, status: 'Active',
        description: 'Royal Biryani Experience' },
      { id: '5', name: 'Subway',
        cuisineType: 'Healthy',
        city: 'Hyderabad', address: 'Gachibowli',
        averageRating: 4.1, totalRatings: 760,
        averageDeliveryTimeMinutes: 25,
        minimumOrderAmount: 150,
        isOpen: false, status: 'Active',
        description: 'Fresh sandwiches and salads' },
      { id: '6', name: "Domino's Pizza",
        cuisineType: 'Pizza',
        city: 'Hyderabad', address: 'Kondapur',
        averageRating: 4.4, totalRatings: 2100,
        averageDeliveryTimeMinutes: 35,
        minimumOrderAmount: 199,
        isOpen: true, status: 'Active',
        description: 'Pizza and more' },
    ] as Restaurant[];
  }
}