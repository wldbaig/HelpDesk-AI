import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const token = inject(AuthService).token();
  return next(token ? request.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : request);
};
export const errorInterceptor: HttpInterceptorFn = (request, next) => {
  const snack = inject(MatSnackBar); const auth = inject(AuthService); const router = inject(Router);
  return next(request).pipe(catchError((error: HttpErrorResponse) => {
    if (error.status === 401 && !request.url.includes('/auth/')) { auth.logout(); void router.navigateByUrl('/login'); }
    const message = error.error?.message || (error.status === 0 ? 'Cannot reach the API. Is the backend running?' : 'Something went wrong. Please try again.');
    snack.open(message, 'Dismiss', { duration: 5000, panelClass: ['error-snackbar'] });
    return throwError(() => error);
  }));
};
