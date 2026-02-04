import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { TokenService } from '../services/token.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const tokenService = inject(TokenService);
  const token = tokenService.getAccessToken();

  // Skip adding token for login/signup endpoints
  if (req.url.includes('api/login') ||
    req.url.includes('/api/createaccount') ||
    req.url.includes('/api/forgotpassword') ||
    req.url.includes('/api/sso-redirect')) {
    return next(req);
  }

  // Add Authorization header if token exists
  if (token) {
    req = req.clone({
      setHeaders: {
        Authorization: 'Bearer ' + token
      }
    });
  }

  return next(req);
};
