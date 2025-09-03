import { ApplicationConfig, APP_INITIALIZER } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { routes } from './app.routes';
import { TranslationService } from './services/translation.service';
import { localeInterceptor } from './interceptors/locale.interceptor';

export function initTranslationsFactory(translationService: TranslationService) {
  return () => translationService.loadLangPromise();
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
  provideHttpClient(withInterceptors([localeInterceptor])),
    {
      provide: APP_INITIALIZER,
      useFactory: initTranslationsFactory,
      deps: [TranslationService],
      multi: true
    }
  ]
};

