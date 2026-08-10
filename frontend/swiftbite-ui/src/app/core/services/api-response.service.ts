import { Injectable } from '@angular/core';

/**
 * ✅ Helper service to extract data from ApiResponse<T>
 */
@Injectable({
  providedIn: 'root'
})
export class ApiResponseService {
  
  /**
   * Extract data from standardized response
   */
  extractData<T>(response: any): T | null {
    try {
      if (!response) return null;
      
      // ✅ Check if it's standardized format
      if (response?.success && 'data' in response) {
        return response.data as T || null;
      }
      
      // ✅ Fallback: response might be just data
      return response as T;
    } catch (error) {
      console.error('Error extracting response data:', error);
      return null;
    }
  }

  /**
   * Check if response is successful
   */
  isSuccess(response: any): boolean {
    return response?.success === true;
  }

  /**
   * Get error message from response
   */
  getErrorMessage(response: any): string {
    return response?.message || 'Unknown error occurred';
  }

  /**
   * Get trace ID for debugging
   */
  getTraceId(response: any): string | null {
    return response?.traceId || null;
  }
}