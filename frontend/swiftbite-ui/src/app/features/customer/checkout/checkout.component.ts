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
  selectedAddr = signal<any>(null);
  paymentMethod = signal<string>('UPI');
  loading      = signal(false);
  error        = signal('');
  step         = signal<'address' | 'payment' | 'confirm'>('address');

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
    this.userSvc.getAddresses().subscribe({
      next: addrs => {
        this.addresses.set(addrs);
        const def = addrs.find(a => a.isDefault);
        if (def) this.selectedAddr.set(def);
        else if (addrs.length > 0)
          this.selectedAddr.set(addrs[0]);
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
    this.userSvc.addAddress(this.newAddress).subscribe({
      next: () => {
        this.loadAddresses();
        this.showAddressForm.set(false);
      },
      error: () => this.error.set('Failed to add address')
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
    
    if (!this.selectedAddr()) {
      this.error.set('Please select a delivery address');
      return;
    }
    this.loading.set(true);
    this.error.set('');
    const user = this.authSvc.currentUser();


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
        this.loading.set(false);
        this.openRazorpay(payData, order);
      },
      error: () => {
        // Mock payment for demo
        this.loading.set(false);
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
            this.cartSvc.clearCart();
            this.router.navigate(
              ['/orders', order.id, 'track']);
          },
          error: () => {
            this.error.set('Payment verification failed!');
          }
        });
      },

      modal: {
        ondismiss: () => {
          this.error.set(
            'Payment cancelled. Your order is saved — retry from orders.');
        }
      }
    };

    const rzp = new Razorpay(options);
    rzp.open();
  }
}