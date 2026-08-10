import { Component, OnInit, OnDestroy, AfterViewInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { OrderService, Order, OrderStatus } from '../../../core/services/order.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ToastService } from '../../../core/services/toast.service';
import { TrackingService, LocationUpdate } from '../../../core/services/tracking.service';
import { Subscription } from 'rxjs';
import * as L from 'leaflet';

export enum PaymentStatus {
  Pending = 1, Paid = 2, Failed = 3, Refunded = 4
}

// ✅ Fix Leaflet default icon
const iconDefault = L.icon({
  iconRetinaUrl: 'assets/marker-icon-2x.png',
  iconUrl:       'assets/marker-icon.png',
  shadowUrl:     'assets/marker-shadow.png',
  iconSize:    [25, 41],
  iconAnchor:  [12, 41],
  popupAnchor: [1, -34],
  shadowSize:  [41, 41]
});
L.Marker.prototype.options.icon = iconDefault;

@Component({
  selector:    'app-order-tracking',
  standalone:  true,
  imports:     [CommonModule],
  templateUrl: './order-tracking.component.html',
  styleUrl:    './order-tracking.component.scss'
})
export class OrderTrackingComponent
  implements OnInit, AfterViewInit, OnDestroy {

  private route       = inject(ActivatedRoute);
  public  router      = inject(Router);
  private orderSvc    = inject(OrderService);
  private notifSvc    = inject(NotificationService);
  private toast       = inject(ToastService);
  private trackingSvc = inject(TrackingService);

  order      = signal<any>(null);
  loading    = signal(true);
  orderId    = '';

  // Map state
  partnerName  = signal<string>('');
  orderStatus  = signal<string>('');
  lastUpdated  = signal<string>('');
  isTracking   = signal<boolean>(false);
  showMap      = signal<boolean>(false);

  private map!:          L.Map;
  private partnerMarker: L.Marker | null = null;
  private routeLine:     L.Polyline | null = null;
  private sub!:          Subscription;
  private mapInitialized = false;

  steps = [
    { status: OrderStatus.Pending,         label: 'Order Placed',      icon: '🛒', desc: 'Your order has been received' },
    { status: OrderStatus.Confirmed,       label: 'Confirmed',         icon: '✅', desc: 'Restaurant accepted your order' },
    { status: OrderStatus.Preparing,       label: 'Preparing',         icon: '👨‍🍳', desc: 'Chef is cooking your food' },
    { status: OrderStatus.Ready,           label: 'Ready',             icon: '📦', desc: 'Food is packed and ready' },
    { status: OrderStatus.PickedUp,        label: 'Picked Up',         icon: '🛵', desc: 'Delivery partner picked up' },
    { status: OrderStatus.OutForDelivery,  label: 'Out for Delivery',  icon: '🚀', desc: 'On the way to your door!' },
    { status: OrderStatus.Delivered,       label: 'Delivered',         icon: '🎉', desc: 'Enjoy your meal!' },
  ];

  statusOrder = [
    OrderStatus.Pending, OrderStatus.Confirmed, OrderStatus.Preparing,
    OrderStatus.Ready, OrderStatus.PickedUp, OrderStatus.OutForDelivery,
    OrderStatus.Delivered
  ];

  private pollInterval: any;

  ngOnInit(): void {
    this.orderId = this.route.snapshot.paramMap.get('id')!;
    this.loadOrder();
    this.notifSvc.connectSignalR();
    this.pollInterval = setInterval(() => this.loadOrder(), 15000);
  }

  ngAfterViewInit(): void {
    // Map initialized after order loads and status is PickedUp+
  }

  loadOrder(): void {
    this.orderSvc.getOrderById(this.orderId).subscribe({
      next: order => {
        const prev = this.order()?.status;
        this.order.set(order);
        this.loading.set(false);
        if (prev && prev !== order.status) {
          this.onStatusChange(order.status);
        }
        // ✅ Show map when partner is on the way
        const trackingStatuses = [
          OrderStatus.PickedUp,
          OrderStatus.OutForDelivery
        ];
        if (trackingStatuses.includes(order.status)) {
          this.showMap.set(true);
          setTimeout(() => this.initMapIfNeeded(order), 300);
          this.startTracking();
        }
      },
      error: () => {
        this.loading.set(false);
        this.order.set(this.getMockOrder());
      }
    });
  }

  private initMapIfNeeded(order: any): void {
    if (this.mapInitialized) return;
    const el = document.getElementById('tracking-map');
    if (!el) return;

  // ✅ ADD: Check map height is set before init
  if (el.offsetHeight === 0) {
    setTimeout(() => this.initMapIfNeeded(order), 500);
    return;
  }
    this.mapInitialized = true;

    const customerLat = order.deliveryLatitude  || 17.3850;
    const customerLng = order.deliveryLongitude || 78.4867;

    this.map = L.map('tracking-map', {
      center:      [customerLat, customerLng],
      zoom:        14,
      zoomControl: true
    });

    // ✅ ADD THIS — forces Leaflet to recalculate container size
    setTimeout(() => {
      this.map.invalidateSize();
    }, 200);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '© OpenStreetMap contributors',
      maxZoom: 19
    }).addTo(this.map);

    // 🏠 Customer marker
    L.marker([customerLat, customerLng], {
      icon: L.divIcon({
        html: `<div class="map-marker map-marker--blue">🏠</div>`,
        className: '', iconSize: [40, 40], iconAnchor: [20, 40]
      })
    }).bindPopup(`<b>Delivery Location</b><br>${order.deliveryAddress}`).addTo(this.map);
  }

  private startTracking(): void {
    if (this.isTracking()) return;

    this.trackingSvc.startTracking(this.orderId).then(() => {
      this.isTracking.set(true);
    });

    this.sub = this.trackingSvc.location$.subscribe(loc => {
      if (loc) this.updateMap(loc);
    });
  }

  private updateMap(loc: LocationUpdate): void {
    if (!this.map) return;

    this.partnerName.set(loc.partnerName);
    this.orderStatus.set(loc.status);
    this.lastUpdated.set(new Date(loc.updatedAt).toLocaleTimeString());

    const latlng: L.LatLngExpression = [loc.latitude, loc.longitude];

    if (this.partnerMarker) {
      this.partnerMarker.setLatLng(latlng);
    } else {
      this.partnerMarker = L.marker(latlng, {
        icon: L.divIcon({
          html: `<div class="map-marker map-marker--green">🛵</div>`,
          className: '', iconSize: [44, 44], iconAnchor: [22, 44]
        })
      }).bindPopup(`<b>${loc.partnerName}</b><br>Your delivery partner`).addTo(this.map);
    }

    const order = this.order();
    if (order?.deliveryLatitude && order?.deliveryLongitude) {
      if (this.routeLine) this.map.removeLayer(this.routeLine);
      this.routeLine = L.polyline(
        [latlng, [order.deliveryLatitude, order.deliveryLongitude]],
        { color: '#f97316', weight: 3, dashArray: '8 8', opacity: 0.8 }
      ).addTo(this.map);
    }

    this.map.panTo(latlng, { animate: true, duration: 1 });
  }

  onStatusChange(status: OrderStatus): void {
    const step = this.steps.find(s => s.status === status);
    if (step) this.toast.success(`${step.icon} ${step.label} — ${step.desc}`);
  }

  getCurrentStepIndex(): number {
    const currentStatus = this.order()?.status ?? OrderStatus.Pending;
    return this.statusOrder.indexOf(currentStatus);
  }

  isStepDone(status: OrderStatus): boolean {
    return this.statusOrder.indexOf(status) <= this.getCurrentStepIndex();
  }

  isStepActive(status: OrderStatus): boolean {
    return this.order()?.status === status;
  }

  getProgressPercent(): number {
    const idx = this.getCurrentStepIndex();
    return Math.round((idx / (this.statusOrder.length - 1)) * 100);
  }

  isCancelled(): boolean {
    const s = this.order()?.status;
    return s === OrderStatus.Cancelled || s === OrderStatus.Refunded;
  }

  canCancel(): boolean {
    const s = this.order()?.status;
    return s === OrderStatus.Pending || s === OrderStatus.Confirmed;
  }

  cancelOrder(): void {
    if (!confirm('Are you sure you want to cancel this order?')) return;
    this.orderSvc.cancelOrder(this.orderId).subscribe({
      next: () => { this.toast.success('Order cancelled.'); this.loadOrder(); },
      error: () => this.toast.error('Cannot cancel order now.')
    });
  }

  getPaymentStatusLabel(status: PaymentStatus | number): string {
    const labels: Record<number, string> = {
      [PaymentStatus.Pending]: 'Pending', [PaymentStatus.Paid]: 'Paid',
      [PaymentStatus.Failed]: 'Failed',   [PaymentStatus.Refunded]: 'Refunded',
    };
    return labels[status] || 'Unknown';
  }

  getOrderStatusLabel(status: OrderStatus): string {
    const labels: Record<OrderStatus, string> = {
      [OrderStatus.Pending]: 'Pending',         [OrderStatus.Confirmed]: 'Confirmed',
      [OrderStatus.Preparing]: 'Preparing',     [OrderStatus.Ready]: 'Ready',
      [OrderStatus.PickedUp]: 'Picked Up',      [OrderStatus.OutForDelivery]: 'Out for Delivery',
      [OrderStatus.Delivered]: 'Delivered',     [OrderStatus.Cancelled]: 'Cancelled',
      [OrderStatus.Refunded]: 'Refunded',
    };
    return labels[status] || 'Unknown';
  }

  getMockOrder(): any {
    return {
      id: this.orderId,
      restaurantName: 'Paradise Biryani',
      status: OrderStatus.PickedUp,
      paymentStatus: PaymentStatus.Paid,
      placedAt: new Date(),
      deliveryAddress: 'Kukatpally, Hyderabad',
      deliveryLatitude:  17.3850,
      deliveryLongitude: 78.4867,
      subTotal: 350, deliveryFee: 30, taxes: 17.5, totalAmount: 397.5,
      items: [{ name: 'Chicken Biryani', quantity: 2, unitPrice: 175, totalPrice: 350 }],
      statusHistory: [
        { status: OrderStatus.Pending,   note: 'Order placed',         timestamp: new Date() },
        { status: OrderStatus.Confirmed, note: 'Restaurant confirmed', timestamp: new Date() },
        { status: OrderStatus.Preparing, note: 'Cooking started',      timestamp: new Date() },
        { status: OrderStatus.PickedUp,  note: 'Partner picked up',    timestamp: new Date() },
      ]
    };
  }

  ngOnDestroy(): void {
    clearInterval(this.pollInterval);
    this.sub?.unsubscribe();
    if (this.isTracking()) this.trackingSvc.stopTracking(this.orderId);
    if (this.map) this.map.remove();
  }
}