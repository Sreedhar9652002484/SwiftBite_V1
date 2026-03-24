import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class UserService {
  private http = inject(HttpClient);
  private api  = environment.apiGatewayUrl;

  getProfile(): Observable<any> {
    return this.http.get(
      `${this.api}/api/users/profile`);
  }

  createProfile(request: any): Observable<any> {
    return this.http.post(
      `${this.api}/api/users/profile`, request);
  }

  updateProfile(request: any): Observable<any> {
    return this.http.put(
      `${this.api}/api/users/profile`, request);
  }

  getAddresses(): Observable<any[]> {
    return this.http.get<any[]>(
      `${this.api}/api/users/addresses`);
  }

  addAddress(request: any): Observable<any> {
    return this.http.post(
      `${this.api}/api/users/addresses`, request);
  }

  deleteAddress(id: string): Observable<any> {
    return this.http.delete(
      `${this.api}/api/users/addresses/${id}`);
  }

  setDefaultAddress(id: string): Observable<any> {
    return this.http.put(
      `${this.api}/api/users/addresses/${id}/default`, {});
  }

  getPreferences(): Observable<any> {
    return this.http.get(
      `${this.api}/api/users/preferences`);
  }

  updatePreferences(request: any): Observable<any> {
    return this.http.put(
      `${this.api}/api/users/preferences`, request);
  }
}