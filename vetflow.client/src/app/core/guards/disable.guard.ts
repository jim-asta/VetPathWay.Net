import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';

export const disableGuard: CanActivateFn = () => {
  const router = inject(Router);

  // Optional: redirect somewhere safe
  router.navigate(['/']);

  // Block route activation
  return false;
};
