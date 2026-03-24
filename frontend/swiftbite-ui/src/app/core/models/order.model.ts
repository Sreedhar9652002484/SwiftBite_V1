export interface Order {
  id: string;
  customerId: string;
  customerName: string;
  restaurantId: string;
  restaurantName: string;
  deliveryAddress: string;
  items: OrderItem[];
  subTotal: number;
  deliveryFee: number;
  taxes: number;
  totalAmount: number;
  status: OrderStatus;
  paymentStatus: string;
  placedAt: Date;
  statusHistory: OrderStatusHistory[];
}

export interface OrderItem {
  menuItemId: string;
  name: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}

export interface OrderStatusHistory {
  status: string;
  note: string;
  timestamp: Date;
}

export enum OrderStatus {
  Pending         = 'Pending',
  Confirmed       = 'Confirmed',
  Preparing       = 'Preparing',
  Ready           = 'Ready',
  PickedUp        = 'PickedUp',
  OutForDelivery  = 'OutForDelivery',
  Delivered       = 'Delivered',
  Cancelled       = 'Cancelled',
  Refunded        = 'Refunded'
}

export interface PlaceOrderRequest {
  restaurantId: string;
  restaurantName: string;
  deliveryAddress: string;
  deliveryCity: string;
  deliveryPinCode: string;
  items: { menuItemId: string; name: string;
           quantity: number; unitPrice: number; }[];
  paymentMethod: string;
}