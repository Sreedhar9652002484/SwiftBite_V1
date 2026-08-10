import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule }      from '@angular/common';
import { FormsModule }       from '@angular/forms';
import { AuthService }       from '../../../core/auth/auth.service';
import { RestaurantService } from '../../../core/services/restaurant.service';

interface MenuCategory {
  id:           string;
  name:         string;
  description?: string;
  displayOrder: number;
  items:        MenuItem[];
}

interface MenuItem {
  id:                     string;
  name:                   string;
  description:            string;
  price:                  number;
  isVegetarian:           boolean;
  isVegan:                boolean;
  isGlutenFree:           boolean;
  preparationTimeMinutes: number;
  imageUrl?:              string;
  categoryId:             string;
}

@Component({
  selector: 'app-menu-manager',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './menu-manager.component.html',
  styleUrls:   ['./menu-manager.component.scss'],
})
export class MenuManagerComponent implements OnInit {

  private auth    = inject(AuthService);
  private restSvc = inject(RestaurantService);

  categories           = signal<MenuCategory[]>([]);
  loading              = signal(true);
  saving               = signal(false);
  errorMsg             = signal<string | null>(null);
  successMsg           = signal<string | null>(null);
  showAddCategory      = signal(false);
  expandedCategory     = signal<string | null>(null);
  addingItemToCatId    = signal<string | null>(null);

  newCategory = { name: '', description: '', displayOrder: 1 };

  newItem = {
    name: '', description: '', price: 0,
    isVegetarian: false, isVegan: false, isGlutenFree: false,
    preparationTimeMinutes: 15, imageUrl: '',
  };

  totalItems = computed(() =>
    this.categories().reduce((sum, c) => sum + (c.items?.length ?? 0), 0)
  );

  private get rid(): string | null {
    const u = this.auth.currentUser();
    return u?.['restaurantId'] ?? u?.['restaurant_id'] ?? null;
  }

  ngOnInit(): void { this.loadMenu(); }

  loadMenu(): void {
    if (!this.rid) { this.loading.set(false); return; }
    this.loading.set(true);
    this.restSvc.getMenuCategories(this.rid).subscribe({
      next:  cats => { this.categories.set(cats); this.loading.set(false); },
      error: ()   => { this.loading.set(false); this.toast('error', 'Failed to load menu.'); },
    });
  }

  toggleCategory(id: string): void {
    this.expandedCategory.set(this.expandedCategory() === id ? null : id);
  }

  // ── Category ──────────────────────────────────────────────
  submitCategory(): void {
    if (!this.rid || !this.newCategory.name.trim()) return;
    this.saving.set(true);
    this.restSvc.createMenuCategory(this.rid, {
      name:         this.newCategory.name.trim(),
      description:  this.newCategory.description || undefined,
      displayOrder: this.newCategory.displayOrder,
    }).subscribe({
      next: cat => {
        this.categories.update(l => [...l, { ...cat, items: [] }]);
        this.newCategory = { name: '', description: '', displayOrder: this.categories().length + 1 };
        this.showAddCategory.set(false);
        this.saving.set(false);
        this.toast('success', 'Category added!');
      },
      error: () => { this.saving.set(false); this.toast('error', 'Failed to add category.'); },
    });
  }

  deleteCategory(catId: string, name: string): void {
    if (!confirm(`Delete "${name}" and all its items?`) || !this.rid) return;
    this.restSvc.deleteMenuCategory(this.rid, catId).subscribe({
      next:  () => { this.categories.update(l => l.filter(c => c.id !== catId)); this.toast('success', 'Category deleted.'); },
      error: () => this.toast('error', 'Failed to delete category.'),
    });
  }

  // ── Items ─────────────────────────────────────────────────
  openAddItem(catId: string): void {
    this.addingItemToCatId.set(catId);
    this.expandedCategory.set(catId);
    this.newItem = { name: '', description: '', price: 0, isVegetarian: false, isVegan: false, isGlutenFree: false, preparationTimeMinutes: 15, imageUrl: '' };
  }

  cancelAddItem(): void { this.addingItemToCatId.set(null); }

  submitItem(catId: string): void {
    if (!this.rid || !this.newItem.name.trim() || this.newItem.price <= 0) return;
    this.saving.set(true);
    this.restSvc.createMenuItem({
      categoryId:             catId,
      restaurantId:           this.rid,
      name:                   this.newItem.name.trim(),
      description:            this.newItem.description.trim(),
      price:                  this.newItem.price,
      isVegetarian:           this.newItem.isVegetarian,
      isVegan:                this.newItem.isVegan,
      isGlutenFree:           this.newItem.isGlutenFree,
      preparationTimeMinutes: this.newItem.preparationTimeMinutes,
      imageUrl:               this.newItem.imageUrl || undefined,
    }).subscribe({
      next: item => {
        this.categories.update(l =>
          l.map(c => c.id === catId ? { ...c, items: [...(c.items ?? []), item] } : c)
        );
        this.addingItemToCatId.set(null);
        this.saving.set(false);
        this.toast('success', 'Item added!');
      },
      error: () => { this.saving.set(false); this.toast('error', 'Failed to add item.'); },
    });
  }

  deleteItem(itemId: string, catId: string, name: string): void {
    if (!confirm(`Delete "${name}"?`)) return;
    this.restSvc.deleteMenuItem(itemId).subscribe({
      next: () => {
        this.categories.update(l =>
          l.map(c => c.id === catId ? { ...c, items: c.items.filter(i => i.id !== itemId) } : c)
        );
        this.toast('success', 'Item deleted.');
      },
      error: () => this.toast('error', 'Failed to delete item.'),
    });
  }

  // ── Diet helpers ──────────────────────────────────────────
  dietLabel(item: MenuItem): string {
    if (item.isVegan)      return 'Vegan';
    if (item.isVegetarian) return 'Veg';
    return 'Non-Veg';
  }
  dietClass(item: MenuItem): string {
    if (item.isVegan)      return 'tag-vegan';
    if (item.isVegetarian) return 'tag-veg';
    return 'tag-nonveg';
  }

  private toast(type: 'success' | 'error', msg: string): void {
    if (type === 'success') { this.successMsg.set(msg); setTimeout(() => this.successMsg.set(null), 3000); }
    else                    { this.errorMsg.set(msg);   setTimeout(() => this.errorMsg.set(null),   4000); }
  }
}