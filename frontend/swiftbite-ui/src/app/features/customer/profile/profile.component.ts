import { Component, OnInit, inject, signal }
  from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { UserService }
  from '../../../core/services/user.service';
import { AuthService }
  from '../../../core/auth/auth.service';
import { ToastService }
  from '../../../core/services/toast.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './profile.component.html'
})
export class ProfileComponent implements OnInit {
  private userSvc = inject(UserService);
  private toast   = inject(ToastService);
  authSvc         = inject(AuthService);
  router          = inject(Router);

  activeTab   = signal<'profile'|'addresses'|'preferences'>('profile');
  loading     = signal(false);
  saving      = signal(false);

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
    dietaryPreference: 'None',
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
    'None', 'Vegetarian',
    'Vegan', 'NonVegetarian'
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
        // Mock for demo
        const mock = {
          firstName: this.authSvc.firstName,
          lastName: '',
          email: this.authSvc.userEmail,
          phoneNumber: ''
        };
        this.profile.set(mock);
        this.profileForm.firstName = mock.firstName;
        this.loading.set(false);
      }
    });
  }

  loadAddresses(): void {
    this.userSvc.getAddresses().subscribe({
      next: a => this.addresses.set(a),
      error: () => this.addresses.set([
        {
          id: 'mock-1', label: 'Home',
          fullAddress: 'Kukatpally, Hyderabad',
          city: 'Hyderabad', pinCode: '500072',
          isDefault: true, addressType: 'Home'
        }
      ])
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

  deleteAddress(id: string): void {
    if (!confirm('Delete this address?')) return;
    this.userSvc.deleteAddress(id).subscribe({
      next: () => {
        this.toast.success('Address deleted');
        this.loadAddresses();
      },
      error: () =>
        this.toast.error('Failed to delete address')
    });
  }

  setDefault(id: string): void {
    this.userSvc.setDefaultAddress(id).subscribe({
      next: () => {
        this.toast.success('✅ Default address set!');
        this.loadAddresses();
      },
      error: () =>
        this.toast.error('Failed to set default')
    });
  }

  savePreferences(): void {
    this.saving.set(true);
    this.userSvc.updatePreferences(this.prefForm)
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