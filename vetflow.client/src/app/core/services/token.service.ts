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
    if (tokens.access_token) {
      sessionStorage.setItem(this.ACCESS_TOKEN_KEY, tokens.access_token);
      this.tokenSubject.next(tokens.access_token);
    }
    if (tokens.id_token) {
      sessionStorage.setItem(this.ID_TOKEN_KEY, tokens.id_token);
    }
    if (tokens.refresh_token) {
      sessionStorage.setItem(this.REFRESH_TOKEN_KEY, tokens.refresh_token);
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
