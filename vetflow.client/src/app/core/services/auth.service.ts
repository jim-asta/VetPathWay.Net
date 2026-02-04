import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { TokenService } from '@services/token.service';
import { AuthToken } from '@DTOs/auth-token.dto';
import { Idp } from '@enums/idp';

@Injectable({ providedIn: 'root' })
export class AuthService {
  constructor(private http: HttpClient, private tokenService: TokenService, private router: Router) { }

  loginLocal(email: string, password: string): Observable<AuthToken> {
    return this.http.post<AuthToken>('/api/login', { email, password }).pipe(
      tap(tokens => this.tokenService.setTokens(tokens))
    );
  }

  signup(email: string, password: string, passwordConfirm: string): Observable<any> {
    return this.http.post('/api/createaccount', { email, password, passwordConfirm });
  }

  forgotPassword(userEmail: string): Observable<any> {
    return this.http.post('/api/forgotpassword', { email: userEmail });
  }

  logout(): void {
    this.tokenService.clearTokens();
    this.router.navigate(['/login']);
  }

  isAuthenticated(): boolean {
    return this.tokenService.isAuthenticated();
  }

  loginSso(idp: Idp) {
    // Redirect to Entra B2C SSO with PKCE
    window.location.href = '/api/ssoredirect/' + idp.toLowerCase();
  }

  handleSsoCallback(token?: string): void {
    if (token) {
      // If token is in URL (Option 1)
      this.tokenService.setTokens({ accessToken: token, refreshToken: '' });
      this.router.navigate(['/dashboard']);
    } else {
      // If token is in cookie (Option 2), back-end should set it
      // Just redirect to dashboard, token will be in httpOnly cookie
      this.router.navigate(['/dashboard']);
    }
  }
}
