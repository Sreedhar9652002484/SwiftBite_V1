import { Injectable, signal, computed } from '@angular/core';
import { MenuItem } from '../models/restaurant.model';

export interface CartItem {
  menuItemId:  string;
  name:        string;
  price:       number;
  quantity:    number;
  imageUrl?:   string;
  isVegetarian: boolean;
}

@Injectable({ providedIn: 'root' })
export class CartService {

  private _items     = signal<CartItem[]>([]);
  restaurantId       = signal<string>('');
  restaurantName     = signal<string>('');

  // ✅ Computed values — auto-update when items change
  items     = this._items.asReadonly();
  itemCount = computed(() =>
    this._items().reduce((s, i) => s + i.quantity, 0));
  subTotal  = computed(() =>
    this._items().reduce((s, i) => s + i.price * i.quantity, 0));
  deliveryFee = computed(() =>
    this._items().length > 0 ? 30 : 0);
  taxes     = computed(() =>
    Math.round(this.subTotal() * 0.05 * 100) / 100);
  total     = computed(() =>
    this.subTotal() + this.deliveryFee() + this.taxes());

  addItem(item: MenuItem, restId: string, restName: string): void {
    // ✅ Different restaurant? Clear cart first!
    if (this.restaurantId() &&
        this.restaurantId() !== restId) {
      if (!confirm(
        'Your cart has items from another restaurant. ' +
        'Clear cart and add new item?')) return;
      this.clearCart();
    }

    this.restaurantId.set(restId);
    this.restaurantName.set(restName);

    const existing = this._items().find(
      i => i.menuItemId === item.id);

    if (existing) {
      this._items.update(items =>
        items.map(i => i.menuItemId === item.id
          ? { ...i, quantity: i.quantity + 1 }
          : i));
    } else {
      this._items.update(items => [...items, {
        menuItemId:   item.id,
        name:         item.name,
        price:        item.price,
        quantity:     1,
        imageUrl:     item.imageUrl,
        isVegetarian: item.isVegetarian
      }]);
    }
  }

  removeItem(menuItemId: string): void {
    this._items.update(items => {
      const existing = items.find(
        i => i.menuItemId === menuItemId);
      if (!existing) return items;
      if (existing.quantity === 1)
        return items.filter(
          i => i.menuItemId !== menuItemId);
      return items.map(i =>
        i.menuItemId === menuItemId
          ? { ...i, quantity: i.quantity - 1 }
          : i);
    });
  }

  getQuantity(menuItemId: string): number {
    return this._items().find(
      i => i.menuItemId === menuItemId)?.quantity ?? 0;
  }

  clearCart(): void {
    this._items.set([]);
    this.restaurantId.set('');
    this.restaurantName.set('');
  }
}