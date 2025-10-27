import { Component, EffectRef, ElementRef, OnDestroy, OnInit, ViewChild, effect, inject } from '@angular/core';
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
import { CurrentUserService } from '@app/services/current-user.service';
import { finalize } from 'rxjs/operators';
import { ShowForRolesDirective } from '@app/directives/show-for-roles.directive';
import { UserRole } from '@app/_models/user';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, TranslatePipe, ShowForRolesDirective],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css']
})
export class ProfileComponent implements OnInit, OnDestroy {
  profileSvc = inject(ProfileService);
  toastr = inject(ToastrService);
  translationService = inject(TranslationService);
  cityService = inject(CityService);
  fb = inject(FormBuilder);
  currentUserSvc = inject(CurrentUserService);
  userRole = UserRole;
  
  editMode = false;
  draftDescription = '';
  saving = false;
  avatarUploading = false;
  avatarLoadFailed = false;
  readonly maxAvatarSize = 5 * 1024 * 1024; // 5 MB

  editBank = false;
  draftBank = '';
  savingBank = false;
  ibanInvalid = false;
  cities: string[] = this.cityService.cities() ?? [];

  editAddress = false;
  savingAddress = false;
  addressForm: FormGroup = buildAddressFormGroup(this.fb);

  editPickups = false;
  savingPickups = false;
  selectedPickups: string[] = [];
  draftPickups: string[] = [];
  readonly pickupOptions = [
    { value: 'smallPickup', label: 'profile.pickups.options.smallPickup' },
    { value: 'pickup', label: 'profile.pickups.options.pickup' },
    { value: 'container', label: 'profile.pickups.options.container' },
    { value: 'specialOrder', label: 'profile.pickups.options.specialOrder' }
  ];
  private readonly pickupStoragePrefix = 'wf_pickup_types_';

  @ViewChild('avatarInput') avatarInput?: ElementRef<HTMLInputElement>;
  private profileEffect: EffectRef = effect(() => {
    const profile = this.profileSvc.profile();
    if (profile) {
      this.avatarLoadFailed = false;
      if (!this.editPickups) {
        const stored = this.loadStoredPickups(profile.userId);
        this.selectedPickups = [...stored];
        this.draftPickups = [...stored];
      }
    } else if (!this.editPickups) {
      this.selectedPickups = [];
      this.draftPickups = [];
    }
  });

  ngOnInit(): void {
    this.avatarLoadFailed = false;
    this.profileSvc.refresh();
  }

  ngOnDestroy(): void {
    this.profileEffect.destroy();
  }

  triggerAvatarPicker() {
    this.avatarInput?.nativeElement.click();
  }

  onAvatarSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input?.files && input.files.length > 0 ? input.files[0] : null;
    if (!file) {
      return;
    }

    if (!file.type.startsWith('image/')) {
      this.handleAvatarError('profile.avatar.invalidFile');
      this.resetAvatarInput();
      return;
    }

    if (file.size > this.maxAvatarSize) {
      this.handleAvatarError('profile.avatar.fileTooLarge');
      this.resetAvatarInput();
      return;
    }

    this.avatarUploading = true;
    this.avatarLoadFailed = false;

    this.profileSvc
      .uploadAvatar(file)
      .pipe(
        finalize(() => {
          this.avatarUploading = false;
          this.resetAvatarInput();
        })
      )
      .subscribe({
        next: () => {
          this.toastr.success(this.translationService.translate('profile.avatar.uploadSuccess'));
          this.profileSvc.refresh();
        }
      });
  }

  onAvatarError() {
    this.avatarLoadFailed = true;
  }

  onAvatarLoad() {
    this.avatarLoadFailed = false;
  }

  private resetAvatarInput() {
    if (this.avatarInput?.nativeElement) {
      this.avatarInput.nativeElement.value = '';
    }
  }

  private handleAvatarError(key: string) {
    this.toastr.error(this.translationService.translate(key));
  }

  avatarSource(p: { avatarUrl: string | null } | null): string | null {
    if (p?.avatarUrl) {
      return p.avatarUrl;
    }
    return this.currentUserSvc.user()?.avatarUrl ?? null;
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

  startEditPickups() {
    this.draftPickups = [...this.selectedPickups];
    this.editPickups = true;
    this.savingPickups = false;
  }

  cancelPickups() {
    this.editPickups = false;
    this.savingPickups = false;
    this.draftPickups = [...this.selectedPickups];
  }

  onPickupToggle(value: string, event: Event) {
    const input = event.target as HTMLInputElement | null;
    if (!input) {
      return;
    }

    const checked = input.checked;
    if (checked) {
      if (!this.draftPickups.includes(value)) {
        this.draftPickups = [...this.draftPickups, value];
      }
    } else {
      this.draftPickups = this.draftPickups.filter(item => item !== value);
    }
  }

  savePickups() {
    const profile = this.profileSvc.profile();
    if (!profile) {
      return;
    }

    this.savingPickups = true;
    const payload = [...this.draftPickups];
    const stored = this.storePickups(profile.userId, payload);
    this.selectedPickups = [...stored];
    this.draftPickups = [...stored];
    this.editPickups = false;
    this.savingPickups = false;
    this.toastr.success(this.translationService.translate('profile.pickups.saved'));
  }

  private pickupStorageKey(userId: string): string {
    return `${this.pickupStoragePrefix}${userId}`;
  }

  private loadStoredPickups(userId: string): string[] {
    if (typeof window === 'undefined') {
      return [];
    }
    try {
      const allowed = new Set(this.pickupOptions.map(option => option.value));
      const raw = window.localStorage.getItem(this.pickupStorageKey(userId));
      if (!raw) {
        return [];
      }
      const parsed = JSON.parse(raw);
      if (!Array.isArray(parsed)) {
        return [];
      }
      return parsed
        .map(entry => (typeof entry === 'string' ? entry.trim() : String(entry ?? '').trim()))
        .filter(entry => entry.length > 0 && allowed.has(entry));
    } catch {
      return [];
    }
  }

  private storePickups(userId: string, values: string[]): string[] {
    const order = this.pickupOptions.map(option => option.value);
    const allowed = new Set(order);
    const unique = Array.from(new Set(values.filter(value => value && value.length > 0 && allowed.has(value))));
    unique.sort((a, b) => order.indexOf(a) - order.indexOf(b));
    if (typeof window === 'undefined') {
      return unique;
    }
    try {
      const key = this.pickupStorageKey(userId);
      if (unique.length === 0) {
        window.localStorage.removeItem(key);
      } else {
        window.localStorage.setItem(key, JSON.stringify(unique));
      }
      return unique;
    } catch {
      return unique;
    }
  }
}

