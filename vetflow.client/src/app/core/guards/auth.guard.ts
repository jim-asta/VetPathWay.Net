import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { CanActivateFn } from '@angular/router';
import { TokenService } from '@services/token.service';

export const authGuard: CanActivateFn = (route, state) => {
 
  const tokenService = inject(TokenService);
  const router = inject(Router);

  if (tokenService.isAuthenticated()) {
    return true;
  }

  router.navigate(['/login']);   // Redirect to login if unauthorized (possibly replace with an Unauthorized page??)
  return false;
};
