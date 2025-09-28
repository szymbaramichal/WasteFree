import { HttpInterceptorFn, HttpRequest, HttpHandlerFn } from '@angular/common/http';

function getToken(): string | null {
  try { return localStorage.getItem('authToken'); } catch { return null; }
}

export const authInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn) => {
  const token = getToken();
  if (!token) return next(req);
  const cloned = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
  return next(cloned);
};
