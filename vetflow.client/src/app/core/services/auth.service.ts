import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { TokenService } from '@services/token.service';
import { AuthToken } from '@DTOs/auth-token.dto';

@Injectable({ providedIn: 'root' })
export class AuthService {
  constructor(private http: HttpClient, private tokenService: TokenService, private router: Router) { }

  loginLocal(email: string, password: string): Observable<AuthToken> {
    return this.http.post<AuthToken>('/api/login', { email, password }).pipe(
      tap(tokens => this.tokenService.setTokens(tokens))
    );
  }

  signup(email: string, password: string, passwordConfirm: string) {
    return this.http.post('/api/createaccount', { email, password, passwordConfirm });
  }

  resetPassword(userEmail: string, newPassword: string) {
    return this.http.post('/api/reset-password', { userEmail, newPassword });
  }

  logout(): void {
    this.tokenService.clearTokens();
    this.router.navigate(['/login']);
  }

  isAuthenticated(): boolean {
    return this.tokenService.isAuthenticated();
  }

  loginSso(idp: 'google' | 'microsoft') {
    // Redirect to Entra B2C SSO with PKCE
    window.location.href = `/api/sso-redirect/${idp}`;
  }
}
