import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  if (!request.url.startsWith('/api/')) return next(request);
  const auth = inject(AuthService);
  const router = inject(Router);
  const isLogin = request.url === '/api/Auth/login';
  const token = isLogin ? null : auth.token;
  const authenticated = token ? request.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : request;
  return next(authenticated).pipe(catchError((error: HttpErrorResponse) => {
    if (error.status === 401 && !isLogin) {
      auth.clearSession();
      void router.navigate(['/'], { queryParams: { reason: 'expired' } });
    }
    if (error.status === 403) auth.permissionError.set('Bu işlem için yetkiniz bulunmuyor.');
    return throwError(() => error);
  }));
};
