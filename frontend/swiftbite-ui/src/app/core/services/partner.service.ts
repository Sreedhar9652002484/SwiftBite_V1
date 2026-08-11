import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ApiResponseService } from './api-response.service';

export interface PartnerApplicationRequest {
  requestedRole: 'RestaurantAdmin' | 'DeliveryPartner';
  phone: string;
  businessName?: string;
  city?: string;
  vehicleType?: string;
  licenseNumber?: string;
  note?: string;
}

export interface PartnerApplication {
  id: string;
  requestedRole: string;
  status: string;
  businessName?: string;
  city?: string;
  vehicleType?: string;
  licenseNumber?: string;
  phone: string;
  note?: string;
  createdAt: string;
  applicantName: string;
  applicantEmail: string;
}

@Injectable({ providedIn: 'root' })
export class PartnerService {
  private http = inject(HttpClient);
  private apiResponseService = inject(ApiResponseService);
  private api = environment.apiGatewayUrl;

  apply(request: PartnerApplicationRequest): Observable<{ id: string }> {
    return this.http.post<any>(`${this.api}/api/partner-applications`, request).pipe(
      map(response => {
        const data = this.apiResponseService.extractData<{ id: string }>(response);
        if (!data) throw new Error('Failed to submit application');
        return data;
      })
    );
  }

  listPending(): Observable<PartnerApplication[]> {
    return this.http.get<any>(`${this.api}/api/partner-applications?status=Pending`).pipe(
      map(response => this.apiResponseService.extractData<PartnerApplication[]>(response) || [])
    );
  }

  approve(id: string): Observable<void> {
    return this.http.post<any>(`${this.api}/api/partner-applications/${id}/approve`, {}).pipe(
      map(() => undefined)
    );
  }

  reject(id: string, note?: string): Observable<void> {
    return this.http.post<any>(`${this.api}/api/partner-applications/${id}/reject`, { note }).pipe(
      map(() => undefined)
    );
  }
}
