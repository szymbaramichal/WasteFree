import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { TranslatePipe } from '@app/pipes/translate.pipe';
import { ProfileService } from '@app/services/profile.service';
import { ToastrService } from 'ngx-toastr';
import { TranslationService } from '@app/services/translation.service';
import { CityService } from '@app/services/city.service';
import { FormBuilder, FormGroup } from '@angular/forms';
import { buildAddressFormGroup } from '@app/forms/address-form';
import { Address } from '@app/_models/address';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, TranslatePipe],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css']
})
export class ProfileComponent implements OnInit {
  profileSvc = inject(ProfileService);
  toastr = inject(ToastrService);
  translationService = inject(TranslationService);
  cityService = inject(CityService);
  fb = inject(FormBuilder);
  
  editMode = false;
  draftDescription = '';
  saving = false;

  editBank = false;
  draftBank = '';
  savingBank = false;
  ibanInvalid = false;
  citiesLoading = false;
  citiesLoadError = false;
  cities: string[] = [];

  editAddress = false;
  savingAddress = false;
  addressForm: FormGroup = buildAddressFormGroup(this.fb);

  ngOnInit(): void {
    this.profileSvc.refresh();
    this.loadCities();
  }

  startEdit(current: string | undefined | null) {
    this.draftDescription = current || '';
    this.editMode = true;
  }

  cancel() {
    this.editMode = false;
    this.saving = false;
  }

  save() {
    const value = this.draftDescription ?? '';
    this.saving = true;
    this.profileSvc.updateDescription(value).subscribe({
      next: () => {
        this.saving = false;
        this.editMode = false;
        this.profileSvc.refresh();
        this.toastr.success(this.translationService.translate('success.update'));
      },
      error: () => {
        this.saving = false;
      }
    });
  }

  startEditBank(current: string | undefined | null) {
    this.draftBank = current || '';
    this.editBank = true;
  }

  cancelBank() {
    this.editBank = false;
    this.savingBank = false;
  }

  saveBank() {
    const value = (this.draftBank ?? '').replace(/\s+/g, '').toUpperCase();
    this.ibanInvalid = !this.validateIban(value);
    if (this.ibanInvalid) {
      return;
    }
    this.savingBank = true;
    this.profileSvc.updateProfile({ bankAccountNumber: value }).subscribe({
      next: () => {
        this.savingBank = false;
        this.editBank = false;
        this.draftBank = value;
        this.profileSvc.refresh();
      },
      error: () => {
        this.savingBank = false;
      }
    });
  }

  private validateIban(iban: string): boolean {
    const raw = (iban ?? '').toUpperCase();
    const basicOk = /^[A-Z0-9]{15,34}$/.test(raw);
    const rearranged = raw.slice(4) + raw.slice(0, 4);
    const expanded = rearranged.replace(/[A-Z]/g, c => (c.charCodeAt(0) - 55).toString());
    let remainder = 0;
    for (let i = 0; i < expanded.length; i++) {
      remainder = (remainder * 10 + (expanded.charCodeAt(i) - 48)) % 97;
    }
    return basicOk && remainder === 1;
  }

  startEditAddress(address: Address | undefined | null) {
    const existing: Address = {
      city: address?.city ?? '',
      postalCode: address?.postalCode ?? '',
      street: address?.street ?? ''
    };
    this.addressForm.reset(existing);
    if (!existing.city && this.cities.length > 0) {
      this.addressForm.get('city')?.setValue(this.cities[0]);
    }
    if (!this.cities.length && !this.citiesLoading) {
      this.loadCities();
    }
    this.editAddress = true;
    this.savingAddress = false;
  }

  cancelAddress() {
    this.editAddress = false;
    this.savingAddress = false;
    const currentAddress = this.profileSvc.profile()?.address;
    const fallback: Address = {
      city: currentAddress?.city ?? '',
      postalCode: currentAddress?.postalCode ?? '',
      street: currentAddress?.street ?? ''
    };
    this.addressForm.reset(fallback);
  }

  saveAddress() {
    if (this.addressForm.invalid) {
      this.addressForm.markAllAsTouched();
      return;
    }

    this.savingAddress = true;
    const formValue = this.addressForm.value as Address;
    const address: Address = {
      city: formValue.city?.trim() ?? '',
      postalCode: formValue.postalCode?.trim() ?? '',
      street: formValue.street?.trim() ?? ''
    };

    this.profileSvc.updateProfile({ address }).subscribe({
      next: () => {
        this.savingAddress = false;
        this.editAddress = false;
        this.profileSvc.refresh();
        this.toastr.success(this.translationService.translate('profile.addressSaved'));
      },
      error: () => {
        this.savingAddress = false;
        this.toastr.error(this.translationService.translate('profile.addressSaveError'));
      }
    });
  }

  private loadCities(): void {
    this.citiesLoading = true;
    this.citiesLoadError = false;
    this.cities = [];

    this.cityService.getCitiesList().subscribe({
      next: (res) => {
        const list = res?.resultModel ?? [];
        this.cities = Array.isArray(list) ? list : [];
        this.citiesLoading = false;
        if (this.editAddress && !this.addressForm.get('city')?.value && this.cities.length > 0) {
          this.addressForm.get('city')?.setValue(this.cities[0]);
        }
      },
      error: () => {
        this.citiesLoading = false;
        this.citiesLoadError = true;
        this.toastr.error(this.translationService.translate('profile.cityLoadError'));
      }
    });
  }
}

