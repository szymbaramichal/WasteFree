import { AbstractControl, FormBuilder, FormGroup, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';

// Matches Polish-style postal codes such as 12-345.
const POSTAL_CODE_PATTERN = /^[0-9]{2}-[0-9]{3}$/;
// Requires at least one digit in the street field so a house number is provided.
const STREET_WITH_NUMBER_PATTERN = /\d+/;

const CITY_POSTAL_PATTERNS: Record<string, RegExp> = {
  cracow: /^(30|31)-\d{3}$/,
  krakow: /^(30|31)-\d{3}$/,
  warsaw: /^(00|01|02|03|04|05)-\d{3}$/,
  warszawa: /^(00|01|02|03|04|05)-\d{3}$/
};

const normalizeCity = (city: string): string => city
  .normalize('NFD')
  .replace(/[\u0300-\u036f]/g, '')
  .toLowerCase();

const cityPostalValidator: ValidatorFn = (group: AbstractControl): ValidationErrors | null => {
  if (!(group instanceof FormGroup)) {
    return null;
  }

  const cityControl = group.get('city');
  const postalControl = group.get('postalCode');

  if (!postalControl) {
    return null;
  }

  const sanitizedErrors = { ...(postalControl.errors ?? {}) };
  delete sanitizedErrors['cityMismatch'];

  const city = String(cityControl?.value ?? '').trim();
  const postal = String(postalControl.value ?? '').trim();

  const requiredPattern = city ? CITY_POSTAL_PATTERNS[normalizeCity(city)] : undefined;
  const hasMismatch = !!postal && (!requiredPattern || !requiredPattern.test(postal));

  if (hasMismatch) {
    postalControl.setErrors({ ...sanitizedErrors, cityMismatch: true });
  } else {
    postalControl.setErrors(Object.keys(sanitizedErrors).length ? sanitizedErrors : null);
  }

  return null;
};

export function buildAddressFormGroup(fb: FormBuilder) {
  return fb.group({
    city: ['', Validators.required],
    postalCode: ['', [Validators.required, Validators.pattern(POSTAL_CODE_PATTERN)]],
    street: ['', [Validators.required, Validators.pattern(STREET_WITH_NUMBER_PATTERN)]]
  }, { validators: cityPostalValidator });
}
