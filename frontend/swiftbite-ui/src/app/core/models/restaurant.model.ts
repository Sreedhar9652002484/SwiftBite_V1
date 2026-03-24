export interface Restaurant {
  id: string;
  name: string;
  description: string;
  city: string;
  address: string;
  logoUrl?: string;
  bannerUrl?: string;
  cuisineType: string;
  averageRating: number;
  totalRatings: number;
  minimumOrderAmount: number;
  averageDeliveryTimeMinutes: number;
  isOpen: boolean;
  status: string;
}

export interface MenuCategory {
  id: string;
  restaurantId: string;
  name: string;
  description?: string;
  displayOrder: number;
  menuItems: MenuItem[];
}

export interface MenuItem {
  id: string;
  categoryId: string;
  restaurantId: string;
  name: string;
  description?: string;
  price: number;
  imageUrl?: string;
  isVegetarian: boolean;
  isVegan: boolean;
  isBestseller: boolean;
  status: string;
  preparationTimeMinutes: number;
}