import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ApiResponseService } from './api-response.service';

// ── Enums ────────────────────────────────────────────────────────────────────
export enum CuisineType {
  Indian         = 1,
  Chinese        = 2,
  Italian        = 3,
  Mexican        = 4,
  American       = 5,
  Thai           = 6,
  Japanese       = 7,
  Mediterranean  = 8,
  FastFood       = 9,
  Desserts       = 10,
  Beverages      = 11,
  MultiCuisine   = 12,
  Biryani        = 13,
  NorthIndian    = 14,
  SouthIndian    = 15,
  Continental    = 16,
  Vietnamese     = 17,
  Korean         = 18,
  MiddleEastern  = 19,
  Turkish        = 20,
  Spanish        = 21,
  Greek          = 22,
  Portuguese     = 23,
  Lebanese       = 24,
  Pakistani      = 25,
  Afghan         = 26,
  Seafood        = 27,
  Vegan          = 28,
  Bakery         = 29,
  Cafe           = 30,
  Pizza          = 31,
  Burgers        = 32,
  Healthy        = 33,
}

// ── Models ───────────────────────────────────────────────────────────────────
export interface Restaurant {
  id: string;
  ownerId: string;
  name: string;
  description: string;
  phoneNumber: string;
  email: string;
  address: string;
  city: string;
  pinCode: string;
  latitude: number;
  longitude: number;
  cuisineType: CuisineType | string; // allow string label from mock
  minimumOrderAmount: number;
  averageDeliveryTimeMinutes: number;
  isOpen: boolean;
  rating: number;
  reviewCount: number;
  averageRating: number;
  totalRatings: number;
  status: number;
  createdAt: Date;
  bannerUrl?: string;
  logoUrl?: string;
}

/**
 * Unified MenuItem — supports BOTH:
 *  • Backend shape  : isAvailable (boolean)
 *  • Old model shape: status ('Available' | 'Unavailable')
 *
 * Both fields are optional so neither shape breaks.
 */
export interface MenuItem {
  id: string;
  name: string;
  restaurantId: string;
  description: string;
  price: number;
  categoryId: string;
  imageUrl?: string;
  isAvailable?: boolean;       // backend field
  status?: string;             // old model field ('Available' | 'Unavailable')
  isVegetarian: boolean;
  isVegan: boolean;
  isBestseller: boolean;
  isGlutenFree: boolean;
  preparationTimeMinutes: number;
}

export interface MenuCategory {
  id: string;
  name: string;
  description?: string;
  displayOrder: number;
  restaurantId: string;
  items: MenuItem[];        // raw API field
  menuItems: MenuItem[];    // normalised alias (always populated)
}

export interface ToggleStatusResponse {
  isOpen: boolean;
}

// ── Service ──────────────────────────────────────────────────────────────────
@Injectable({ providedIn: 'root' })
export class RestaurantService {
  private http               = inject(HttpClient);
  private apiResponseService = inject(ApiResponseService);
  private api                = environment.apiGatewayUrl;

  // ── Normaliser: ensures menuItems is always set ─────────────────────────
  private normaliseCategory(cat: MenuCategory, restaurantId: string): MenuCategory {
    const menuItems = cat.menuItems?.length ? cat.menuItems : (cat.items ?? []);
    return { ...cat, restaurantId, items: menuItems, menuItems };
  }

  // ── Read ─────────────────────────────────────────────────────────────────
  getByCity(city: string): Observable<Restaurant[]> {
    return this.http
      .get<any>(`${this.api}/api/restaurants/city/${city}`)
      .pipe(
        map(res => this.apiResponseService.extractData<Restaurant[]>(res) ?? [])
      );
  }

  getAll(): Observable<Restaurant[]> {
    return this.http
      .get<any>(`${this.api}/api/restaurants`)
      .pipe(
        map(res => this.apiResponseService.extractData<Restaurant[]>(res) ?? [])
      );
  }

  getById(id: string): Observable<Restaurant> {
    return this.http
      .get<any>(`${this.api}/api/restaurants/${id}`)
      .pipe(
        map(res => {
          const r = this.apiResponseService.extractData<Restaurant>(res);
          if (!r) throw new Error('Restaurant not found');
          return r;
        })
      );
  }

  getMenu(restaurantId: string): Observable<MenuCategory[]> {
    return this.http
      .get<any>(`${this.api}/api/restaurants/${restaurantId}/menu`)
      .pipe(
        map(res => {
          const cats = this.apiResponseService.extractData<MenuCategory[]>(res) ?? [];
          return cats.map(c => this.normaliseCategory(c, restaurantId));
        })
      );
  }

  getMenuCategories(restaurantId: string): Observable<MenuCategory[]> {
    return this.getMenu(restaurantId); // same endpoint, reuse normaliser
  }

  searchItems(keyword: string): Observable<MenuItem[]> {
    return this.http
      .get<any>(`${this.api}/api/restaurants/items/search`, { params: { keyword } })
      .pipe(
        map(res => this.apiResponseService.extractData<MenuItem[]>(res) ?? [])
      );
  }

  // ── Write ────────────────────────────────────────────────────────────────
  toggleRestaurantStatus(id: string): Observable<ToggleStatusResponse> {
    return this.http
      .put<any>(`${this.api}/api/restaurants/${id}/toggle`, {})
      .pipe(
        map(res => {
          const r = this.apiResponseService.extractData<ToggleStatusResponse>(res);
          if (!r) throw new Error('Failed to toggle status');
          return r;
        })
      );
  }

  updateRestaurant(id: string, payload: Partial<Restaurant>): Observable<Restaurant> {
    return this.http
      .put<any>(`${this.api}/api/restaurants/${id}`, payload)
      .pipe(
        map(res => {
          const r = this.apiResponseService.extractData<Restaurant>(res);
          if (!r) throw new Error('Failed to update restaurant');
          return r;
        })
      );
  }

  createMenuCategory(
    restaurantId: string,
    payload: { name: string; description?: string; displayOrder: number }
  ): Observable<MenuCategory> {
    return this.http
      .post<any>(`${this.api}/api/restaurants/${restaurantId}/menu`, payload)
      .pipe(
        map(res => {
          const cat = this.apiResponseService.extractData<MenuCategory>(res);
          if (!cat) throw new Error('Failed to create category');
          return this.normaliseCategory(cat, restaurantId);
        })
      );
  }

  deleteMenuCategory(restaurantId: string, categoryId: string): Observable<void> {
    return this.http
      .delete<any>(`${this.api}/api/restaurants/${restaurantId}/menu/${categoryId}`)
      .pipe(
        map(res => {
          if (!this.apiResponseService.isSuccess(res))
            throw new Error('Failed to delete category');
        })
      );
  }

  createMenuItem(payload: any): Observable<MenuItem> {
    return this.http
      .post<any>(`${this.api}/api/restaurants/items`, payload)
      .pipe(
        map(res => {
          const item = this.apiResponseService.extractData<MenuItem>(res);
          if (!item) throw new Error('Failed to create menu item');
          return item;
        })
      );
  }

  deleteMenuItem(itemId: string): Observable<void> {
    return this.http
      .delete<any>(`${this.api}/api/restaurants/items/${itemId}`)
      .pipe(
        map(res => {
          if (!this.apiResponseService.isSuccess(res))
            throw new Error('Failed to delete item');
        })
      );
  }
}