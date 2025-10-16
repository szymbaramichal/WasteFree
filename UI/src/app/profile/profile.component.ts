import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '../pipes/translate.pipe';
import { ProfileService } from '../services/profile.service';
import { ToastrService } from 'ngx-toastr';
import { TranslationService } from '../services/translation.service';
import { CityService } from '../services/city.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslatePipe],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css']
})
export class ProfileComponent implements OnInit {
  profileSvc = inject(ProfileService);
  toastr = inject(ToastrService);
  translationService = inject(TranslationService);
  cityService = inject(CityService);
  
  editMode = false;
  draftDescription = '';
  saving = false;

  editBank = false;
  draftBank = '';
  savingBank = false;
  ibanInvalid = false;
  citySaving = false;
  citiesLoading = false;
  citiesLoadError = false;
  cities: string[] = [];

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

  onCityChanged(city: string) {
    if (this.citySaving) {
      return;
    }

    const current = this.profileSvc.profile()?.city ?? '';
    if (city === current) {
      return;
    }

    this.citySaving = true;
    this.profileSvc.updateProfile({ city }).subscribe({
      next: () => {
        this.citySaving = false;
        this.profileSvc.refresh();
        this.toastr.success(this.translationService.translate('profile.citySaved'));
      },
      error: () => {
        this.citySaving = false;
        this.toastr.error(this.translationService.translate('profile.citySaveError'));
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
      },
      error: () => {
        this.citiesLoading = false;
        this.citiesLoadError = true;
        this.toastr.error(this.translationService.translate('profile.cityLoadError'));
      }
    });
  }
}

