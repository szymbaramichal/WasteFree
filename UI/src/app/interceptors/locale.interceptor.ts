import { HttpInterceptorFn, HttpRequest, HttpHandlerFn } from '@angular/common/http';

function detectLang(): string {
  try {
    const stored = localStorage.getItem('lang');
    if (stored) return stored;
  } catch {
    // ignore
  }
  return 'pl';
}

export const localeInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn) => {
  const lang = detectLang();
  const acceptLang = lang === 'pl' ? 'pl-PL' : 'en-US';
  const cloned = req.clone({ setHeaders: { 'Accept-Language': acceptLang } });
  return next(cloned);
};
