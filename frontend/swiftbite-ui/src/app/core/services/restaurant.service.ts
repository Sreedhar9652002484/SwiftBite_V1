import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Restaurant, MenuCategory } from '../models/restaurant.model';

// ── Owner-specific types ──────────────────────────────────────
export interface MenuItem {
  id:          string;
  name:        string;
  description: string;
  price:       number;
  categoryId:  string;
  imageUrl?:   string;
  isAvailable: boolean;
}

export interface CreateMenuItemRequest {
  name:        string;
  description: string;
  price:       number;
  categoryId:  string;
  imageUrl?:   string;
}

export interface UpdateMenuItemRequest extends Partial<CreateMenuItemRequest> {
  isAvailable?: boolean;
}

// ─────────────────────────────────────────────────────────────

@Injectable({ providedIn: 'root' })
export class RestaurantService {
  private http = inject(HttpClient);
  private api  = environment.apiGatewayUrl;

  // ── Customer: get restaurants ─────────────────────────────
  getByCity(city: string): Observable<Restaurant[]> {
    return this.http.get<Restaurant[]>(
      `${this.api}/api/restaurants?city=${city}`);
  }

  getAll(): Observable<Restaurant[]> {
    return this.http.get<Restaurant[]>(
      `${this.api}/api/restaurants`);
  }

  getById(id: string): Observable<Restaurant> {
    return this.http.get<Restaurant>(
      `${this.api}/api/restaurants/${id}`);
  }

  // ── Customer: menu + search ───────────────────────────────
  getMenu(restaurantId: string): Observable<MenuCategory[]> {
    return this.http.get<MenuCategory[]>(
      `${this.api}/api/restaurants/${restaurantId}/menu`);
  }

  searchItems(keyword: string): Observable<any[]> {
    return this.http.get<any[]>(
      `${this.api}/api/restaurants/items/search`,
      { params: { keyword } });
  }

  // ── Owner: restaurant status toggle ──────────────────────
  toggleRestaurantStatus(id: string): Observable<{ isOpen: boolean }> {
    return this.http.put<{ isOpen: boolean }>(
      `${this.api}/api/restaurants/${id}/toggle`, {});
  }

  updateRestaurant(id: string, payload: {
    name:                       string;
    description:                string;
    phoneNumber:                string;
    address:                    string;
    city:                       string;
    pinCode:                    string;
    minimumOrderAmount:         number;
    averageDeliveryTimeMinutes: number;
    cuisineType:                number;
  }): Observable<any> {
    return this.http.put<any>(
      `${this.api}/api/restaurants/${id}`, payload);
  }

  // ── Owner: menu categories ────────────────────────────
  // GET    /api/restaurants/{id}/menu
  // POST   /api/restaurants/{id}/menu
  // DELETE /api/restaurants/{id}/menu/{categoryId}
  getMenuCategories(restaurantId: string): Observable<any[]> {
    return this.http.get<any[]>(
      `${this.api}/api/restaurants/${restaurantId}/menu`);
  }

  createMenuCategory(restaurantId: string, payload: { name: string; description?: string; displayOrder: number }): Observable<any> {
    return this.http.post<any>(
      `${this.api}/api/restaurants/${restaurantId}/menu`, payload);
  }

  deleteMenuCategory(restaurantId: string, categoryId: string): Observable<void> {
    return this.http.delete<void>(
      `${this.api}/api/restaurants/${restaurantId}/menu/${categoryId}`);
  }

  // ── Owner: menu items ─────────────────────────────────
  // POST   /api/restaurants/items
  // DELETE /api/restaurants/items/{id}
  createMenuItem(payload: {
    categoryId: string;
    restaurantId: string;
    name: string;
    description: string;
    price: number;
    isVegetarian: boolean;
    isVegan: boolean;
    isGlutenFree: boolean;
    preparationTimeMinutes: number;
    imageUrl?: string;
  }): Observable<any> {
    return this.http.post<any>(
      `${this.api}/api/restaurants/items`, payload);
  }

  deleteMenuItem(itemId: string): Observable<void> {
    return this.http.delete<void>(
      `${this.api}/api/restaurants/items/${itemId}`);
  }
}