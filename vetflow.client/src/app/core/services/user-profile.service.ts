import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UserProfile } from '@DTOs/user-profile.dto';

@Injectable({ providedIn: 'root' })
export class UserProfileService {
  constructor(private http: HttpClient) { }

  getUserProfile(): Observable<UserProfile> {
    return this.http.get<UserProfile>('/api/userprofile');
  }
}
