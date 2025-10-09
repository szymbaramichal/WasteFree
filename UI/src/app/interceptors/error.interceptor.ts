import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { catchError } from 'rxjs';
import { TranslationService } from '../services/translation.service';
import { extractApiErrorPayload } from '../helpers/api-error.helper';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const toastr = inject(ToastrService);
  const translationService = inject(TranslationService);

  return next(req).pipe(
    catchError(err => {
      if(err) {
        const error = err?.error ?? err;
        switch (err.status) {
          case 400:
            if (typeof error?.errorMessage === 'string' && error.errorMessage.trim())
            {
                toastr.error(error.errorMessage.trim());
                break;
            }
            toastr.error(translationService.translate('error.generic'));
            break;
          case 401:
            toastr.error(translationService.translate('error.401'));
            break;
          case 403:
            if (typeof error?.errorMessage === 'string' && error.errorMessage.trim())
            {
                toastr.error(error.errorMessage.trim());
                break;
            }
            toastr.error(translationService.translate('error.403'));
            break;
          case 404:
            if (typeof error?.errorMessage === 'string' && error.errorMessage.trim())
            {
                toastr.error(error.errorMessage.trim());
                break;
            }
            toastr.error(translationService.translate('error.generic'));
            break;
          case 422:
            const errors = extractApiErrorPayload(error);
            errors.forEach(errorContent => {
              toastr.error(errorContent);
            });
            break;
          default:
            toastr.error(translationService.translate('error.generic'));
            break;
        }
      }
      throw err;
    })
  );
};
