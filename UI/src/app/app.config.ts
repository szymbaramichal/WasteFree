import { ApplicationConfig, APP_INITIALIZER } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { routes } from './app.routes';
import { TranslationService } from './services/translation.service';
import { localeInterceptor } from './interceptors/locale.interceptor';
import { authInterceptor } from './interceptors/auth.interceptor';
import { provideToastr } from 'ngx-toastr';
import { provideAnimations } from '@angular/platform-browser/animations';
import { errorInterceptor } from './interceptors/error.interceptor';
import { registerLocaleData } from '@angular/common';
import localePl from '@angular/common/locales/pl';

registerLocaleData(localePl);

export function initTranslationsFactory(translationService: TranslationService) {
  return () => translationService.loadLangPromise();
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptors([errorInterceptor, authInterceptor, localeInterceptor])),
    {
      provide: APP_INITIALIZER,
      useFactory: initTranslationsFactory,
      deps: [TranslationService],
      multi: true
    },
    provideAnimations(),
    provideToastr({
      positionClass: 'toast-bottom-right',
      preventDuplicates: true
    })
  ]
};

