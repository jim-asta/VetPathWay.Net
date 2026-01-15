import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { environment } from '../environments/environment';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  user: {
    id: string;
    email: string;
    name: string;
  };
}

export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  message?: string;
  errors?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private readonly baseUrl = environment.apiBaseUrl;

  constructor(private http: HttpClient) { }

  /**
   * Test API connection with health check endpoint
   */
  testConnection(): Observable<any> {
    return this.http.get(`${this.baseUrl}/healthz`).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Login user (placeholder for future implementation)
   */
  login(credentials: LoginRequest): Observable<ApiResponse<LoginResponse>> {
    // For skeleton mode, return a mock response
    // In real implementation, this would be:
    // return this.http.post<ApiResponse<LoginResponse>>(`${this.baseUrl}/api/auth/login`, credentials)

    return of({
      success: true,
      data: {
        token: 'mock-jwt-token',
        user: {
          id: '1',
          email: credentials.email,
          name: 'Test User'
        }
      }
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Logout user (placeholder for future implementation)
   */
  logout(): Observable<ApiResponse<any>> {
    // For skeleton mode, return a mock response
    // In real implementation, this would be:
    // return this.http.post<ApiResponse<any>>(`${this.baseUrl}/api/auth/logout`, {})

    return of({
      success: true,
      message: 'Logged out successfully'
    });
  }

  /**
   * Get current user profile (placeholder for future implementation)
   */
  getCurrentUser(): Observable<ApiResponse<any>> {
    // For skeleton mode, return a mock response
    // In real implementation, this would be:
    // return this.http.get<ApiResponse<any>>(`${this.baseUrl}/api/user/profile`)

    return of({
      success: true,
      data: {
        id: '1',
        email: 'user@example.com',
        name: 'Test User',
        role: 'user'
      }
    });
  }

  /**
   * Generic GET request
   */
  get<T>(endpoint: string): Observable<ApiResponse<T>> {
    return this.http.get<ApiResponse<T>>(`${this.baseUrl}/api/${endpoint}`).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Generic POST request
   */
  post<T>(endpoint: string, data: any): Observable<ApiResponse<T>> {
    return this.http.post<ApiResponse<T>>(`${this.baseUrl}/api/${endpoint}`, data).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Generic PUT request
   */
  put<T>(endpoint: string, data: any): Observable<ApiResponse<T>> {
    return this.http.put<ApiResponse<T>>(`${this.baseUrl}/api/${endpoint}`, data).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Generic DELETE request
   */
  delete<T>(endpoint: string): Observable<ApiResponse<T>> {
    return this.http.delete<ApiResponse<T>>(`${this.baseUrl}/api/${endpoint}`).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Handle HTTP errors
   */
  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'An unknown error occurred';

    if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = `Client Error: ${error.error.message}`;
    } else {
      // Server-side error
      errorMessage = `Server Error: ${error.status} - ${error.message}`;

      // Handle specific HTTP status codes
      switch (error.status) {
        case 401:
          errorMessage = 'Unauthorized - Please login again';
          break;
        case 403:
          errorMessage = 'Forbidden - You do not have permission';
          break;
        case 404:
          errorMessage = 'Not Found - The requested resource was not found';
          break;
        case 500:
          errorMessage = 'Internal Server Error - Please try again later';
          break;
      }
    }

    console.error('API Error:', error);
    return throwError(() => new Error(errorMessage));
  }
}
