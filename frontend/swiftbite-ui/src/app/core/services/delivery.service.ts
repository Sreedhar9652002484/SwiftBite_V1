import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ApiResponseService } from './api-response.service';

export interface DeliveryPartner {
  id:               string;
  userId:           string;
  firstName:        string;
  lastName:         string;
  email:            string;
  phoneNumber:      string;
  vehicleType:      string;
  vehicleNumber:    string;
  isAvailable:      boolean;
  rating:           number;
  totalDeliveries:  number;
  totalEarnings:    number;
  status:           string;
  createdAt:        string;
}

export interface DeliveryJob {
  id:              string;
  orderId:         string;
  orderNumber:     string;
  customerName:    string;
  customerPhone:   string;
  restaurantName:  string;
  pickupAddress:   string;
  deliveryAddress: string;
  deliveryCity:    string;
  deliveryFee:     number;
  status:          string;
  assignedAt:      string;
  acceptedAt?:     string;
  pickedUpAt?:     string;
  deliveredAt?:    string;
}

export interface EarningsData {
  totalEarnings:   number;
  totalDeliveries: number;
  rating:          number;
  recentJobs:      DeliveryJob[];
}

export interface RegisterPartnerRequest {
  firstName:     string;
  lastName:      string;
  email:         string;
  phoneNumber:   string;
  vehicleType:   number;
  vehicleNumber: string;
}

@Injectable({ providedIn: 'root' })
export class DeliveryService {
  private http = inject(HttpClient);
  private apiResponseService = inject(ApiResponseService);
  private api = environment.apiGatewayUrl;

  /**
   * Register partner
   */
  register(request: RegisterPartnerRequest): Observable<DeliveryPartner> {
    return this.http.post<any>(`${this.api}/api/delivery/register`, request)
      .pipe(
        map(response => {
          const partner = this.apiResponseService.extractData<DeliveryPartner>(response);
          if (!partner) {
            throw new Error('Failed to register delivery partner');
          }
          return partner;
        })
      );
  }

  /**
   * Get profile
   */
  getProfile(): Observable<DeliveryPartner> {
    return this.http.get<any>(`${this.api}/api/delivery/profile`)
      .pipe(
        map(response => {
          const profile = this.apiResponseService.extractData<DeliveryPartner>(response);
          if (!profile) {
            throw new Error('Failed to fetch profile');
          }
          return profile;
        })
      );
  }

  /**
   * Update availability
   */
  updateAvailability(isAvailable: boolean): Observable<DeliveryPartner> {
    return this.http.put<any>(`${this.api}/api/delivery/availability`, { isAvailable })
      .pipe(
        map(response => {
          const updated = this.apiResponseService.extractData<DeliveryPartner>(response);
          if (!updated) {
            throw new Error('Failed to update availability');
          }
          return updated;
        })
      );
  }

  /**
   * Get all jobs
   */
  getJobs(): Observable<DeliveryJob[]> {
    return this.http.get<any>(`${this.api}/api/delivery/jobs`)
      .pipe(
        map(response => {
          const jobs = this.apiResponseService.extractData<DeliveryJob[]>(response);
          return jobs || [];
        })
      );
  }

  /**
   * Get active jobs
   */
  getActiveJobs(): Observable<DeliveryJob[]> {
    return this.http.get<any>(`${this.api}/api/delivery/jobs/active`)
      .pipe(
        map(response => {
          const jobs = this.apiResponseService.extractData<DeliveryJob[]>(response);
          return jobs || [];
        })
      );
  }

  /**
   * Accept job
   */
  acceptJob(jobId: string): Observable<DeliveryJob> {
    return this.http.put<any>(`${this.api}/api/delivery/jobs/${jobId}/accept`, {})
      .pipe(
        map(response => {
          const job = this.apiResponseService.extractData<DeliveryJob>(response);
          if (!job) {
            throw new Error('Failed to accept job');
          }
          return job;
        })
      );
  }

  /**
   * Update job status
   */
  updateJobStatus(jobId: string, status: number): Observable<DeliveryJob> {
    return this.http.put<any>(`${this.api}/api/delivery/jobs/${jobId}/status`, { status })
      .pipe(
        map(response => {
          const job = this.apiResponseService.extractData<DeliveryJob>(response);
          if (!job) {
            throw new Error('Failed to update job status');
          }
          return job;
        })
      );
  }

  /**
   * Get earnings
   */
  getEarnings(): Observable<EarningsData> {
    return this.http.get<any>(`${this.api}/api/delivery/earnings`)
      .pipe(
        map(response => {
          const earnings = this.apiResponseService.extractData<EarningsData>(response);
          if (!earnings) {
            throw new Error('Failed to fetch earnings');
          }
          return earnings;
        })
      );
  }
}