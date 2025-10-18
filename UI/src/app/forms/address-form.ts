import { FormBuilder, Validators } from '@angular/forms';

export function buildAddressFormGroup(fb: FormBuilder) {
  return fb.group({
    city: ['', Validators.required],
    postalCode: ['', Validators.required],
    street: ['', Validators.required]
  });
}
