import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { RestaurantService } from '../../../core/services/restaurant.service';
import { CartItem, CartService } from '../../../core/services/cart.service';
import { Restaurant, MenuCategory, MenuItem } from '../../../core/models/restaurant.model';

@Component({
  selector: 'app-restaurant-detail',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './restaurant-detail.component.html',
})
export class RestaurantDetailComponent implements OnInit {
  private route      = inject(ActivatedRoute);
  public router     = inject(Router);
  private restSvc    = inject(RestaurantService);
  public  cartSvc    = inject(CartService);

  restaurant  = signal<Restaurant | null>(null);
  menu        = signal<MenuCategory[]>([]);
  loading     = signal(true);
  activeTab   = signal('');
  vegOnly     = signal(false);
  searchQuery = signal('');

  filteredMenu = computed(() => {
    return this.menu().map(cat => ({
      ...cat,
      menuItems: cat.menuItems.filter(item => {
        const matchVeg  = !this.vegOnly() || item.isVegetarian;
        const matchSearch = !this.searchQuery() ||
          item.name.toLowerCase()
            .includes(this.searchQuery().toLowerCase());
        return matchVeg && matchSearch &&
          item.status === 'Available';
      })
    })).filter(cat => cat.menuItems.length > 0);
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.loadRestaurant(id);
    this.loadMenu(id);
  }

  loadRestaurant(id: string): void {
    this.restSvc.getById(id).subscribe({
      next: r => {
        this.restaurant.set(r);
        this.loading.set(false);
      },
     error: () => {
      // ✅ Mock restaurant for demo
      this.restaurant.set({
        id: id,
        name: this.getMockName(id),
        cuisineType: 'Biryani',
        city: 'Hyderabad',
        address: 'Secunderabad',
        averageRating: 4.5,
        totalRatings: 2400,
        averageDeliveryTimeMinutes: 35,
        minimumOrderAmount: 150,
        isOpen: true,
        status: 'Active',
        description: 'Best food in Hyderabad',
      } as any);
      this.loading.set(false);
    }
  });
  }

  loadMenu(id: string): void {
    this.restSvc.getMenu(id).subscribe({
      next: menu => {
        this.menu.set(menu);
        if (menu.length > 0)
          this.activeTab.set(menu[0].id);
      },
       error: () => {
      // ✅ Mock menu for demo
      const mockMenu = this.getMockMenu();
      this.menu.set(mockMenu);
      this.activeTab.set(mockMenu[0].id);
    }
    });
  }

  scrollToCategory(catId: string): void {
    this.activeTab.set(catId);
    document.getElementById(`cat-${catId}`)
      ?.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }

  addToCart(item: MenuItem): void {
    const r = this.restaurant();
    if (!r) return;
    this.cartSvc.addItem(item, r.id, r.name);
  }

  removeFromCart(item: MenuItem): void {
    this.cartSvc.removeItem(item.id);
  }

  goToCheckout(): void {
    this.router.navigate(['/checkout']);
  }

  getStars(rating: number): string[] {
    return Array(5).fill('').map((_, i) =>
      i < Math.floor(rating) ? '★' : '☆');
  }

  increaseCartItem(item: CartItem): void {
  // Find the full MenuItem from menu
  const fullItem = this.menu()
    .flatMap(c => c.menuItems)
    .find(m => m.id === item.menuItemId);

  if (fullItem) this.addToCart(fullItem);
}
getMockName(id: string): string {
  const names: Record<string, string> = {
    '1': 'Paradise Biryani',
    '2': 'Pizza Hut',
    '3': "McDonald's",
    '4': 'Behrouz Biryani',
    '5': 'Subway',
    '6': "Domino's Pizza",
  };
  return names[id] ?? 'Restaurant';
}
getMockMenu(): MenuCategory[] {
  return [
    {
      id: 'cat-1',
      restaurantId: '1',
      name: '🍱 Biryani',
      description: 'Our signature biryanis',
      displayOrder: 1,
      menuItems: [
        {
          id: 'item-1',
          categoryId: 'cat-1',
          restaurantId: '1',
          name: 'Chicken Biryani',
          description:
            'Fragrant basmati rice cooked with tender chicken',
          price: 199,
          isVegetarian: false,
          isVegan: false,
          isBestseller: true,
          status: 'Available',
          preparationTimeMinutes: 25,
        },
        {
          id: 'item-2',
          categoryId: 'cat-1',
          restaurantId: '1',
          name: 'Veg Biryani',
          description:
            'Fresh vegetables with aromatic spices',
          price: 149,
          isVegetarian: true,
          isVegan: false,
          isBestseller: false,
          status: 'Available',
          preparationTimeMinutes: 20,
        },
        {
          id: 'item-3',
          categoryId: 'cat-1',
          restaurantId: '1',
          name: 'Mutton Biryani',
          description:
            'Slow-cooked mutton with saffron rice',
          price: 279,
          isVegetarian: false,
          isVegan: false,
          isBestseller: true,
          status: 'Available',
          preparationTimeMinutes: 35,
        },
      ]
    },
    {
      id: 'cat-2',
      restaurantId: '1',
      name: '🥘 Curries',
      description: 'Rich gravies and curries',
      displayOrder: 2,
      menuItems: [
        {
          id: 'item-4',
          categoryId: 'cat-2',
          restaurantId: '1',
          name: 'Butter Chicken',
          description:
            'Creamy tomato-based chicken curry',
          price: 229,
          isVegetarian: false,
          isVegan: false,
          isBestseller: true,
          status: 'Available',
          preparationTimeMinutes: 20,
        },
        {
          id: 'item-5',
          categoryId: 'cat-2',
          restaurantId: '1',
          name: 'Paneer Tikka Masala',
          description:
            'Cottage cheese in spiced gravy',
          price: 189,
          isVegetarian: true,
          isVegan: false,
          isBestseller: false,
          status: 'Available',
          preparationTimeMinutes: 15,
        },
      ]
    },
    {
      id: 'cat-3',
      restaurantId: '1',
      name: '🥗 Starters',
      description: 'Appetizers and starters',
      displayOrder: 3,
      menuItems: [
        {
          id: 'item-6',
          categoryId: 'cat-3',
          restaurantId: '1',
          name: 'Chicken 65',
          description:
            'Crispy spiced fried chicken',
          price: 169,
          isVegetarian: false,
          isVegan: false,
          isBestseller: false,
          status: 'Available',
          preparationTimeMinutes: 15,
        },
        {
          id: 'item-7',
          categoryId: 'cat-3',
          restaurantId: '1',
          name: 'Veg Spring Rolls',
          description:
            'Crispy rolls with vegetables',
          price: 129,
          isVegetarian: true,
          isVegan: true,
          isBestseller: false,
          status: 'Available',
          preparationTimeMinutes: 10,
        },
      ]
    },
    {
      id: 'cat-4',
      restaurantId: '1',
      name: '🥤 Drinks',
      description: 'Beverages and desserts',
      displayOrder: 4,
      menuItems: [
        {
          id: 'item-8',
          categoryId: 'cat-4',
          restaurantId: '1',
          name: 'Mango Lassi',
          description: 'Thick creamy mango yogurt drink',
          price: 79,
          isVegetarian: true,
          isVegan: false,
          isBestseller: false,
          status: 'Available',
          preparationTimeMinutes: 5,
        },
        {
          id: 'item-9',
          categoryId: 'cat-4',
          restaurantId: '1',
          name: 'Sweet Lassi',
          description: 'Classic chilled yogurt drink',
          price: 59,
          isVegetarian: true,
          isVegan: false,
          isBestseller: false,
          status: 'Available',
          preparationTimeMinutes: 5,
        },
      ]
    },
  ];
}
}