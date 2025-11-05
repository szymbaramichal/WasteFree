import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpErrorResponse } from '@angular/common/http';
import { catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { WalletService } from '@app/services/wallet.service';

function getToken(): string | null {
  try { return localStorage.getItem('authToken'); } catch { return null; }
}

function clearAuthData(): void {
  try {
    localStorage.removeItem('authToken');
    localStorage.removeItem('wf_current_user');
  } catch {}
}

export const authInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn) => {
  const token = getToken();
  const cloned = token ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : req;
  const router = inject(Router);
  const wallet = inject(WalletService);
  return next(cloned).pipe(
    catchError((err: unknown) => {
      if (err instanceof HttpErrorResponse && err.status === 401) {
        clearAuthData();
        wallet.resetState();
        try { router.navigateByUrl('/auth'); } catch {
          try { window.location.href = '/auth'; } catch {}
        }
      }
      return throwError(() => err);
    })
  );
};
