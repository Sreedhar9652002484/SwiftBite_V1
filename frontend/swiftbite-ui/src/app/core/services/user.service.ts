import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError, map, switchMap, tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ApiResponseService } from './api-response.service';  // ✅ ADD

export interface UserProfile {
  id: string;
  authUserId: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber?: string;
  profilePictureUrl?: string;
  dateOfBirth?: Date;
  createdAt?: Date;
}

export interface Address {
  id: string;
  label: string;
  fullAddress: string;
  street: string;
  city: string;
  state: string;
  pinCode: string;
  latitude: number;
  longitude: number;
  addressType: 'Home' | 'Office' | 'Other';
  landmark?: string;
  isDefault: boolean;
}

export enum dietaryPreference{
  None = 0,
  Vegetarian = 1,
  Vegan = 2,
  NonVegetarian = 3,   // ✅ ADD THIS
  Jain = 4,
  Keto = 5,
  GlutenFree = 6
}

export interface UserPreferences {
  dietaryPreference: dietaryPreference;
  emailNotifications: boolean;
  pushNotifications: boolean;
  smsNotifications: boolean;
  preferredCuisines: string[];
  allergiesInfo: string;
}

@Injectable({ providedIn: 'root' })
export class UserService {
  private http = inject(HttpClient);
  private apiResponseService = inject(ApiResponseService);  // ✅ ADD
  private api = environment.apiGatewayUrl;

  /**
   * The backend creates the user's profile row on the first GET /api/users/profile.
   * Address and preference endpoints return "User not found" until then, so a new
   * user who goes straight to checkout (e.g. after Google sign-in) would fail.
   * Make sure that call has happened once per session before them.
   */
  private profileEnsured = false;

  private afterProfile<T>(request: () => Observable<T>): Observable<T> {
    const ensure$: Observable<unknown> = this.profileEnsured
      ? of(null)
      : this.getProfile().pipe(
          tap(() => (this.profileEnsured = true)),
          catchError(() => of(null))   // let the real request report any genuine failure
        );
    return ensure$.pipe(switchMap(request));
  }

  /**
   * Get user profile
   * ✅ UPDATED: Extracts data from ApiResponse<UserProfile>
   */
  getProfile(): Observable<UserProfile> {
    return this.http.get<any>(`${this.api}/api/users/profile`)
      .pipe(
        map(response => {
          const profile = this.apiResponseService.extractData<UserProfile>(response);
          if (!profile) {
            throw new Error('No profile data in response');
          }
          return profile;
        })
      );
  }

  /**
   * Create user profile
   * ✅ UPDATED: Extracts data from ApiResponse<UserProfile>
   */
  createProfile(request: any): Observable<UserProfile> {
    return this.http.post<any>(`${this.api}/api/users/profile`, request)
      .pipe(
        map(response => {
          const profile = this.apiResponseService.extractData<UserProfile>(response);
          if (!profile) {
            throw new Error('Failed to create profile');
          }
          return profile;
        })
      );
  }

  /**
   * Update user profile
   * ✅ UPDATED: Extracts data from ApiResponse<UserProfile>
   */
  updateProfile(request: any): Observable<UserProfile> {
    return this.http.put<any>(`${this.api}/api/users/profile`, request)
      .pipe(
        map(response => {
          const profile = this.apiResponseService.extractData<UserProfile>(response);
          if (!profile) {
            throw new Error('Failed to update profile');
          }
          return profile;
        })
      );
  }

  /**
   * Get all addresses
   * ✅ UPDATED: Extracts data from ApiResponse<Address[]>
   */
  getAddresses(): Observable<Address[]> {
    return this.afterProfile(() => this.http.get<any>(`${this.api}/api/users/addresses`))
      .pipe(
        map(response => {
          const addresses = this.apiResponseService.extractData<Address[]>(response);
          return addresses || [];
        })
      );
  }

  /**
   * Add new address
   * ✅ UPDATED: Extracts data from ApiResponse<Address>
   */
  addAddress(request: any): Observable<Address> {
    return this.afterProfile(() => this.http.post<any>(`${this.api}/api/users/addresses`, request))
      .pipe(
        map(response => {
          const address = this.apiResponseService.extractData<Address>(response);
          if (!address) {
            throw new Error('Failed to add address');
          }
          return address;
        })
      );
  }

  /**
   * Delete address
   * ✅ UPDATED: Handles ApiResponse without data
   */
  deleteAddress(id: string): Observable<void> {
    return this.afterProfile(() => this.http.delete<any>(`${this.api}/api/users/addresses/${id}`))
      .pipe(
        map(response => {
          if (!this.apiResponseService.isSuccess(response)) {
            throw new Error('Failed to delete address');
          }
          return undefined;
        })
      );
  }

  /**
   * Set default address
   * ✅ UPDATED: Handles ApiResponse without data
   */
  setDefaultAddress(id: string): Observable<void> {
    return this.afterProfile(() => this.http.put<any>(
      `${this.api}/api/users/addresses/${id}/default`, {}))
      .pipe(
        map(response => {
          if (!this.apiResponseService.isSuccess(response)) {
            throw new Error('Failed to set default address');
          }
          return undefined;
        })
      );
  }

  /**
   * Get user preferences
   * ✅ UPDATED: Extracts data from ApiResponse<UserPreferences>
   */
  getPreferences(): Observable<UserPreferences> {
    return this.afterProfile(() => this.http.get<any>(`${this.api}/api/users/preferences`))
      .pipe(
        map(response => {
          const preferences = this.apiResponseService.extractData<UserPreferences>(response);
          if (!preferences) {
            throw new Error('No preferences data');
          }
          return preferences;
        })
      );
  }

  /**
   * Update user preferences
   * ✅ UPDATED: Extracts data from ApiResponse<UserPreferences>
   */
  updatePreferences(request: any): Observable<UserPreferences> {
    return this.afterProfile(() => this.http.put<any>(`${this.api}/api/users/preferences`, request))
      .pipe(
        map(response => {
          const preferences = this.apiResponseService.extractData<UserPreferences>(response);
          if (!preferences) {
            throw new Error('Failed to update preferences');
          }
          return preferences;
        })
      );
  }
}