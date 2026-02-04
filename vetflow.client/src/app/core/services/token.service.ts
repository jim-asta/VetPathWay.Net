import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { AuthToken } from '@DTOs/auth-token.dto';

@Injectable({ providedIn: 'root' })
export class TokenService {
  private readonly ACCESS_TOKEN_KEY = 'access_token';
  private readonly ID_TOKEN_KEY = 'id_token';
  private readonly REFRESH_TOKEN_KEY = 'refresh_token';

  private tokenSubject = new BehaviorSubject<string | null>(this.getAccessToken());
  public token$: Observable<string | null> = this.tokenSubject.asObservable();

  getAccessToken(): string | null {
    return sessionStorage.getItem(this.ACCESS_TOKEN_KEY);
  }

  getIdToken(): string | null {
    return sessionStorage.getItem(this.ID_TOKEN_KEY);
  }

  setTokens(tokens: AuthToken): void {
    if (tokens.accessToken) {
      sessionStorage.setItem(this.ACCESS_TOKEN_KEY, tokens.accessToken);
      this.tokenSubject.next(tokens.accessToken);
    }
    if (tokens.idToken) {
      sessionStorage.setItem(this.ID_TOKEN_KEY, tokens.idToken);
    }
    if (tokens.refreshToken) {
      sessionStorage.setItem(this.REFRESH_TOKEN_KEY, tokens.refreshToken);
    }
  }

  clearTokens(): void {
    sessionStorage.removeItem(this.ACCESS_TOKEN_KEY);
    sessionStorage.removeItem(this.ID_TOKEN_KEY);
    sessionStorage.removeItem(this.REFRESH_TOKEN_KEY);
    this.tokenSubject.next(null);
  }

  isAuthenticated(): boolean {
    return !!this.getAccessToken();
  }
}
