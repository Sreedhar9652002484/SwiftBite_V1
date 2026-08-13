import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CartService } from '../../../core/services/cart.service';
import { OrderService } from '../../../core/services/order.service';
import { PaymentService } from '../../../core/services/payment.service';
import { UserService } from '../../../core/services/user.service';
import { AuthService } from '../../../core/auth/auth.service';

declare var Razorpay: any; // ✅ Razorpay global

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './checkout.component.html',
  styleUrl: './checkout.component.scss'
})
export class CheckoutComponent implements OnInit {
  cartSvc     = inject(CartService);
  private orderSvc   = inject(OrderService);
  private paymentSvc = inject(PaymentService);
  private userSvc    = inject(UserService);
  private authSvc    = inject(AuthService);
  public router     = inject(Router);

  addresses    = signal<any[]>([]);
  loadingAddresses = signal(true);
  selectedAddr = signal<any>(null);
  paymentMethod = signal<string>('UPI');
  loading      = signal(false);
  error        = signal('');
  step         = signal<'address' | 'payment' | 'confirm'>('address');

  // Set once an order has been created but payment hasn't completed yet
  // (e.g. the Razorpay modal was dismissed or verification failed). A retry
  // reuses this order instead of placing a duplicate one for the same cart.
  private pendingOrder = signal<any>(null);

  paymentMethods = [
    { id: 'UPI',        label: 'UPI',          icon: '📱' },
    { id: 'Card',       label: 'Credit/Debit', icon: '💳' },
    { id: 'NetBanking', label: 'Net Banking',   icon: '🏦' },
    { id: 'Wallet',     label: 'Wallet',        icon: '👜' },
    { id: 'COD',        label: 'Cash on Delivery', icon: '💵' },
  ];



  get selectedPaymentMethod() {
  return this.paymentMethods.find(
    p => p.id === this.paymentMethod());
}
  // New address form
  showAddressForm = signal(false);
  savingAddress   = signal(false);
  newAddress = {
    label: '', fullAddress: '', street: '',
    city: 'Hyderabad', state: 'Telangana',
    pinCode: '', addressType: 'Home'
  };

  ngOnInit(): void {
    if (this.cartSvc.itemCount() === 0) {
      this.router.navigate(['/home']);
      return;
    }
    this.loadAddresses();
    this.loadRazorpayScript();
  }

  loadAddresses(): void {
    this.loadingAddresses.set(true);
    this.userSvc.getAddresses().subscribe({
      next: addrs => {
        this.addresses.set(addrs);
        const def = addrs.find(a => a.isDefault);
        if (def) this.selectedAddr.set(def);
        else if (addrs.length > 0)
          this.selectedAddr.set(addrs[0]);
        this.loadingAddresses.set(false);
      },
      error: () => {
        // Mock address for demo
        const mock = {
          id: 'mock-1', label: 'Home',
          fullAddress: 'Kukatpally, Hyderabad',
          city: 'Hyderabad', pinCode: '500072',
          isDefault: true, addressType: 'Home'
        };
        this.addresses.set([mock]);
        this.selectedAddr.set(mock);
        this.loadingAddresses.set(false);
      }
    });
  }

  loadRazorpayScript(): void {
    if (document.getElementById('razorpay-script')) return;
    const script = document.createElement('script');
    script.id  = 'razorpay-script';
    script.src = 'https://checkout.razorpay.com/v1/checkout.js';
    document.body.appendChild(script);
  }

  addNewAddress(): void {
    if (this.savingAddress()) return;
    this.savingAddress.set(true);
    this.userSvc.addAddress(this.newAddress).subscribe({
      next: () => {
        this.savingAddress.set(false);
        this.loadAddresses();
        this.showAddressForm.set(false);
      },
      error: () => {
        this.savingAddress.set(false);
        this.error.set('Failed to add address');
      }
    });
  }

  nextStep(): void {
    if (this.step() === 'address')   this.step.set('payment');
    else if (this.step() === 'payment') this.step.set('confirm');
  }

  prevStep(): void {
    if (this.step() === 'payment')  this.step.set('address');
    else if (this.step() === 'confirm') this.step.set('payment');
  }

  // ✅ Main order + payment flow
  async placeOrder(): Promise<void> {
    // Reentrancy guard: the button's [disabled]="loading()" binding covers
    // normal clicks, but this makes placeOrder() itself safe against any
    // click that slips in before Angular re-renders the disabled state.
    if (this.loading()) return;

    if (!this.selectedAddr()) {
      this.error.set('Please select a delivery address');
      return;
    }
    this.error.set('');
    this.loading.set(true);
    const user = this.authSvc.currentUser();

    // If a previous attempt already created an order but payment never
    // completed, resume payment on that order instead of placing a new one.
    const existing = this.pendingOrder();
    if (existing) {
      await this.initiatePayment(existing);
      return;
    }

    try {
      // Step 1: Place order
      const orderReq = {
        restaurantId:    this.cartSvc.restaurantId(),
        restaurantName:  this.cartSvc.restaurantName(),
       customerName: user?.name || 'Customer',     // ✅ ADD THIS
       customerPhone: user?.phone || '9999999999', // ✅ ADD THIS
        deliveryAddress: this.selectedAddr().fullAddress,
        deliveryCity:    this.selectedAddr().city,
        deliveryPinCode: this.selectedAddr().pinCode,
        paymentMethod:   this.paymentMethod(),
        items: this.cartSvc.items().map(i => ({
          menuItemId: i.menuItemId,
          name:       i.name,
          quantity:   i.quantity,
          unitPrice:  i.price
        }))
      };

      this.orderSvc.placeOrder(orderReq).subscribe({
        next: async (order) => {
          if (this.paymentMethod() === 'COD') {
            // COD — no payment needed!
            this.cartSvc.clearCart();
            this.router.navigate(
              ['/orders', order.id, 'track']);
            return;
          }
          // Step 2: Initiate Razorpay
          this.pendingOrder.set(order);
          await this.initiatePayment(order);
        },
        error: (err) => {
          this.error.set(
            err.error?.message || 'Failed to place order');
          this.loading.set(false);
        }
      });
    } catch (err: any) {
      this.error.set(err.message || 'Something went wrong');
      this.loading.set(false);
    }
  }

  private async initiatePayment(order: any): Promise<void> {

    const user = this.authSvc.currentUser();
      const methodMap: any = {
        UPI: 1,
        Card: 2,
        NetBanking: 3,
        Wallet: 4,
        COD: 5
      };

    const payReq = {
      orderId:       order.id,
      customerName:  user?.name || 'Customer',
      customerEmail: user?.email || 'customer@email.com',
      customerPhone: user?.phone || '9999999999',
      amount:        order.totalAmount,
      method:        methodMap[this.paymentMethod()]
    };

    this.paymentSvc.initiatePayment(payReq).subscribe({
      next: (payData) => {
        // Keep the button disabled — the Razorpay modal is opening next and
        // the button must stay locked until payment succeeds, fails, or is
        // cancelled, not just until the payment session is created.
        this.openRazorpay(payData, order);
      },
      error: () => {
        // Mock payment for demo
        this.loading.set(false);
        this.pendingOrder.set(null);
        this.cartSvc.clearCart();
        this.router.navigate(
          ['/orders', order.id, 'track']);
      }
    });
  }

  private openRazorpay(payData: any, order: any): void {
    const options = {
      key:          payData.razorpayKeyId,
      amount:       payData.amount * 100,
      currency:     'INR',
      name:         'SwiftBite',
      description:  `Order from ${this.cartSvc.restaurantName()}`,
      order_id:     payData.razorpayOrderId,
      prefill: {
        name:  this.authSvc.currentUser()?.name,
        email: this.authSvc.currentUser()?.email,
      },
      theme: { color: '#FF6B35' },

      handler: (response: any) => {
        // ✅ Verify payment after success
        this.paymentSvc.verifyPayment({
          razorpayOrderId:   response.razorpay_order_id,
          razorpayPaymentId: response.razorpay_payment_id,
          razorpaySignature: response.razorpay_signature
        }).subscribe({
          next: () => {
            this.loading.set(false);
            this.pendingOrder.set(null);
            this.cartSvc.clearCart();
            this.router.navigate(
              ['/orders', order.id, 'track']);
          },
          error: () => {
            // Order still exists unpaid — leave pendingOrder set so a retry
            // resumes payment on it instead of placing a duplicate order.
            this.loading.set(false);
            this.error.set('Payment verification failed! Click Place Order to retry.');
          }
        });
      },

      modal: {
        ondismiss: () => {
          // Order still exists unpaid — leave pendingOrder set so clicking
          // Place Order again resumes payment on it instead of creating a
          // second order for the same cart.
          this.loading.set(false);
          this.error.set(
            'Payment cancelled. Click Place Order to retry payment for your order.');
        }
      }
    };

    const rzp = new Razorpay(options);
    rzp.open();
  }
}