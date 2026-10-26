import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { catchError, map, of } from 'rxjs';
import { AuthService } from '../services/auth';
import { UserRole } from '../models/auth';

export const authGuard: CanActivateFn = route => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (!auth.token) { auth.clearSession(); return router.createUrlTree(['/']); }
  return auth.loadUser().pipe(
    map(user => {
      const roles = route.data['roles'] as UserRole[] | undefined;
      if (roles && !roles.includes(user.role)) {
        auth.permissionError.set('Bu sayfaya erişim yetkiniz bulunmuyor.');
        return router.createUrlTree(['/dashboard']);
      }
      return true;
    }),
    catchError(() => of(router.createUrlTree(['/'], { queryParams: { reason: 'session' } })))
  );
};
