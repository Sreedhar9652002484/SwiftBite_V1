import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { PartnerService, PartnerApplicationRequest } from '../../../core/services/partner.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-become-partner',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './become-partner.component.html',
})
export class BecomePartnerComponent {
  private partnerSvc = inject(PartnerService);
  private toast = inject(ToastService);
  router = inject(Router);

  submitting = signal(false);
  submitted = signal(false);

  role = signal<'RestaurantAdmin' | 'DeliveryPartner'>('RestaurantAdmin');

  form: PartnerApplicationRequest = {
    requestedRole: 'RestaurantAdmin',
    phone: '',
    businessName: '',
    city: '',
    vehicleType: '',
    licenseNumber: '',
    note: '',
  };

  vehicleTypes = ['Bike', 'Scooter', 'Bicycle', 'Car'];

  setRole(role: 'RestaurantAdmin' | 'DeliveryPartner'): void {
    this.role.set(role);
    this.form.requestedRole = role;
  }

  submit(): void {
    if (!this.form.phone) {
      this.toast.error('Phone number is required.');
      return;
    }
    if (this.role() === 'RestaurantAdmin' && !this.form.businessName) {
      this.toast.error('Business name is required.');
      return;
    }
    if (this.role() === 'DeliveryPartner' && !this.form.vehicleType) {
      this.toast.error('Vehicle type is required.');
      return;
    }

    this.submitting.set(true);
    this.partnerSvc.apply(this.form).subscribe({
      next: () => {
        this.submitting.set(false);
        this.submitted.set(true);
      },
      error: (err) => {
        this.submitting.set(false);
        this.toast.error(err?.error?.message || 'Failed to submit application.');
      },
    });
  }
}
