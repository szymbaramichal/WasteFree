export interface Address {
  city: string;
  postalCode: string;
  street: string;
}

type SupportedLocale = 'pl' | 'en';

const CITY_LOCALIZATION_MAP: Record<string, Record<SupportedLocale, string>> = {
  cracow: { pl: 'Kraków', en: 'Cracow' },
  warsaw: { pl: 'Warszawa', en: 'Warsaw' }
};

export function getLocalizedCityName(city: string | null | undefined, lang: string | null | undefined): string | null {
  if (!city) {
    return null;
  }

  const normalized = city.trim().toLowerCase();
  const dictionary = CITY_LOCALIZATION_MAP[normalized];
  const locale = resolveLocale(lang);
  const localized = dictionary?.[locale];

  if (localized) {
    return localized;
  }

  return city.trim();
}

export function formatLocalizedAddress(address: Address | null | undefined, lang: string | null | undefined): string | null {
  if (!address) {
    return null;
  }

  const street = address.street?.trim() ?? '';
  const localizedCity = getLocalizedCityName(address.city, lang) ?? address.city?.trim() ?? '';
  const postalCode = address.postalCode?.trim() ?? '';

  const cityLine = [postalCode, localizedCity]
    .filter((part) => !!part)
    .join(' ')
    .trim();

  const parts = [street, cityLine].filter((part) => !!part);

  if (!parts.length) {
    return null;
  }

  return parts.join(', ');
}

function resolveLocale(lang: string | null | undefined): SupportedLocale {
  if (!lang) {
    return 'en';
  }

  return lang.toLowerCase().startsWith('pl') ? 'pl' : 'en';
}
