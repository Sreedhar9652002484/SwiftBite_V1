import { Component, OnInit, inject, signal }
  from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { dietaryPreference, UserService }
  from '../../../core/services/user.service';
import { AuthService }
  from '../../../core/auth/auth.service';
import { ToastService }
  from '../../../core/services/toast.service';
import { ConfirmService }
  from '../../../core/services/confirm.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './profile.component.html'
})
export class ProfileComponent implements OnInit {
  private userSvc = inject(UserService);
  private toast   = inject(ToastService);
  private confirmSvc = inject(ConfirmService);
  authSvc         = inject(AuthService);
  router          = inject(Router);

  activeTab   = signal<'profile'|'addresses'|'preferences'>('profile');
  loading     = signal(false);
  saving      = signal(false);
  loadError   = signal(false);

  // Tracks which address currently has a delete/set-default request in
  // flight, so only that address's buttons are disabled during the call.
  addressActionId = signal<string | null>(null);

  profile     = signal<any>(null);
  addresses   = signal<any[]>([]);
  preferences = signal<any>(null);

  // Edit forms
  profileForm = {
    firstName: '', lastName: '',
    phoneNumber: '', profilePictureUrl: ''
  };

  addressForm = {
    label: '', fullAddress: '', street: '',
    city: 'Hyderabad', state: 'Telangana',
    pinCode: '', addressType: 'Home',
    latitude: 0, longitude: 0
  };

    prefForm = {
      dietaryPreference: dietaryPreference.None,
      emailNotifications: true,
      pushNotifications: true,
      smsNotifications: false,
      preferredCuisines: [] as string[]
    };

  showAddressForm = signal(false);
  editingProfile  = signal(false);

  cuisineOptions = [
    'Indian', 'Chinese', 'Italian',
    'Mexican', 'Pizza', 'Burgers',
    'Biryani', 'Healthy', 'Desserts'
  ];

dietaryOptions = [
  { label: '🍽️ No Preference', value: dietaryPreference.None },
  { label: '🟢 Vegetarian', value: dietaryPreference.Vegetarian },
  { label: '🌱 Vegan', value: dietaryPreference.Vegan },
  { label: '🔴 Non-Veg', value: dietaryPreference.NonVegetarian }
];
  addressTypes = ['Home', 'Office', 'Other'];

  ngOnInit(): void {
    this.loadProfile();
    this.loadAddresses();
    this.loadPreferences();
  }

  setTab(tab: string): void {
  this.activeTab.set(
    tab as 'profile' | 'addresses' | 'preferences');
}

  loadProfile(): void {
    this.loading.set(true);
    this.loadError.set(false);
    this.userSvc.getProfile().subscribe({
      next: p => {
        this.profile.set(p);
        this.profileForm = {
          firstName:         p.firstName   ?? '',
          lastName:          p.lastName    ?? '',
          phoneNumber:       p.phoneNumber ?? '',
          profilePictureUrl: p.profilePictureUrl ?? ''
        };
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.loadError.set(true);
        this.toast.error('Failed to load your profile.');
      }
    });
  }

  loadAddresses(): void {
    this.userSvc.getAddresses().subscribe({
      next: a => this.addresses.set(a),
      error: () => {
        this.loadError.set(true);
        this.toast.error('Failed to load your saved addresses.');
      }
    });
  }

  loadPreferences(): void {
    this.userSvc.getPreferences().subscribe({
      next: p => {
        this.preferences.set(p);
        this.prefForm = { ...this.prefForm, ...p };
      },
      error: () => {}
    });
  }

  saveProfile(): void {
    if (this.saving()) return;
    this.saving.set(true);
    this.userSvc.updateProfile(this.profileForm)
      .subscribe({
        next: () => {
          this.toast.success('✅ Profile updated!');
          this.saving.set(false);
          this.editingProfile.set(false);
          this.loadProfile();
        },
        error: () => {
          this.toast.error('Failed to update profile');
          this.saving.set(false);
        }
      });
  }

  saveAddress(): void {
    if (this.saving()) return;
    this.saving.set(true);
    this.userSvc.addAddress(this.addressForm)
      .subscribe({
        next: () => {
          this.toast.success('✅ Address added!');
          this.saving.set(false);
          this.showAddressForm.set(false);
          this.loadAddresses();
          this.resetAddressForm();
        },
        error: () => {
          this.toast.error('Failed to add address');
          this.saving.set(false);
        }
      });
  }

  async deleteAddress(id: string): Promise<void> {
    if (this.addressActionId()) return;
    const ok = await this.confirmSvc.confirm({
      title: 'Delete this address?',
      message: 'You can always add it again later.',
      confirmLabel: 'Delete',
      danger: true,
    });
    if (!ok) return;
    this.addressActionId.set(id);
    this.userSvc.deleteAddress(id).subscribe({
      next: () => {
        this.toast.success('Address deleted');
        this.addressActionId.set(null);
        this.loadAddresses();
      },
      error: () => {
        this.toast.error('Failed to delete address');
        this.addressActionId.set(null);
      }
    });
  }

  setDefault(id: string): void {
    if (this.addressActionId()) return;
    this.addressActionId.set(id);
    this.userSvc.setDefaultAddress(id).subscribe({
      next: () => {
        this.toast.success('✅ Default address set!');
        this.addressActionId.set(null);
        this.loadAddresses();
      },
      error: () => {
        this.toast.error('Failed to set default');
        this.addressActionId.set(null);
      }
    });
  }

  savePreferences(): void {
    if (this.saving()) return;
    this.saving.set(true);
      const request = {
    ...this.prefForm, // reuse everything
    preferredCuisines: this.prefForm.preferredCuisines.join(',') // override only this
  };

    this.userSvc.updatePreferences(request)
      .subscribe({
        next: () => {
          this.toast.success('✅ Preferences saved!');
          this.saving.set(false);
        },
        error: () => {
          this.toast.error('Failed to save preferences');
          this.saving.set(false);
        }
      });
  }

  toggleCuisine(cuisine: string): void {
    const list = [...this.prefForm.preferredCuisines];
    const idx  = list.indexOf(cuisine);
    if (idx > -1) list.splice(idx, 1);
    else          list.push(cuisine);
    this.prefForm.preferredCuisines = list;
  }

  hasCuisine(cuisine: string): boolean {
    return this.prefForm.preferredCuisines
      .includes(cuisine);
  }

  resetAddressForm(): void {
    this.addressForm = {
      label: '', fullAddress: '', street: '',
      city: 'Hyderabad', state: 'Telangana',
      pinCode: '', addressType: 'Home',
      latitude: 0, longitude: 0
    };
  }

  getInitial(): string {
    return this.profile()?.firstName?.charAt(0)
      ?? this.authSvc.firstName?.charAt(0)
      ?? 'U';
  }

}