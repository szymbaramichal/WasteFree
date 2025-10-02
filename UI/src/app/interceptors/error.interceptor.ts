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
        switch (err.status) {
          case 400:
            const p = err?.error;

            if (typeof p?.errorMessage === 'string' && p.errorMessage.trim())
            {
                toastr.error(p.errorMessage.trim());
                break;
            }
            toastr.error(translationService.translate('error.generic'));
            break;
          case 401:
            toastr.error(translationService.translate('error.401'));
            break;
          case 422:
            //TODO
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
