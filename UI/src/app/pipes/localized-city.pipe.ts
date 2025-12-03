import { Pipe, PipeTransform, inject } from '@angular/core';
import { TranslationService } from '@app/services/translation.service';
import { getLocalizedCityName } from '@app/_models/address';

@Pipe({
  name: 'localizedCity',
  standalone: true,
  pure: false
})
export class LocalizedCityPipe implements PipeTransform {
  private readonly translation = inject(TranslationService);

  transform(city: string | null | undefined, fallback?: string): string {
    const lang = this.translation.currentLang;
    const localized = getLocalizedCityName(city, lang);

    if (localized && localized.trim().length > 0) {
      return localized;
    }

    if (fallback && fallback.trim().length > 0) {
      return fallback.trim();
    }

    return city?.trim() ?? '';
  }
}
