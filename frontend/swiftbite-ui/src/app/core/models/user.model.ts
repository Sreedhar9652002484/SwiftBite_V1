export interface UserProfile {
  id: string;
  authUserId: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber?: string;
  profilePictureUrl?: string;
  fullName: string;
}

export interface Address {
  id: string;
  label: string;
  fullAddress: string;
  street: string;
  city: string;
  state: string;
  pinCode: string;
  isDefault: boolean;
  addressType: 'Home' | 'Office' | 'Other';
  latitude?: number;
  longitude?: number;
}

export interface UserPreferences {
  dietaryPreference: string;
  emailNotifications: boolean;
  pushNotifications: boolean;
  smsNotifications: boolean;
  preferredCuisines: string[];
}