import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DeliveryService, DeliveryPartner } from '../../../core/services/delivery.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-delivery-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './delivery-profile.component.html',
  styleUrls: ['./delivery-profile.component.scss'],
})
export class DeliveryProfileComponent implements OnInit {

  private svc      = inject(DeliveryService);
  private toastSvc = inject(ToastService);

  loading     = signal(true);
  saving      = signal(false);
  notRegistered = signal(false);
  profile     = signal<DeliveryPartner | null>(null);

  // Registration form
  regForm = {
    firstName: '', lastName: '', email: '',
    phoneNumber: '', vehicleType: 1, vehicleNumber: '',
  };

  vehicleTypes = [
    { value: 1, label: 'Bike'    },
    { value: 2, label: 'Scooter' },
    { value: 3, label: 'Car'     },
    { value: 4, label: 'Cycle'   },
  ];

  ngOnInit(): void {
    this.svc.getProfile().subscribe({
      next:  p => { this.profile.set(p); this.loading.set(false); },
      error: () => { this.notRegistered.set(true); this.loading.set(false); },
    });
  }

  register(): void {
    this.saving.set(true);
    this.svc.register({
      ...this.regForm,
      vehicleType: Number(this.regForm.vehicleType),
    }).subscribe({
      next: p => {
        this.profile.set(p);
        this.notRegistered.set(false);
        this.saving.set(false);
        this.toastSvc.success('Registration successful! You can now receive jobs.');
      },
      error: () => { this.saving.set(false); this.toastSvc.error('Registration failed. Try again.'); },
    });
  }
}