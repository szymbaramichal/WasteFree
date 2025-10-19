import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { TranslatePipe } from '@app/pipes/translate.pipe';
import { GarbageGroup, GarbageGroupInfo, GarbageGroupRole, UpdateGarbageGroupRequest } from '@app/_models/garbageGroups';
import { GarbageGroupService } from '@app/services/garbage-group.service';
import { TranslationService } from '@app/services/translation.service';
import { ToastrService } from 'ngx-toastr';
import { CurrentUserService } from '@app/services/current-user.service';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { buildAddressFormGroup } from '@app/forms/address-form';
import { CityService } from '@app/services/city.service';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-group-panel',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslatePipe, ReactiveFormsModule],
  templateUrl: './group-panel.component.html',
  styleUrls: ['./group-panel.component.css']
})
export class GroupPanelComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private groupService = inject(GarbageGroupService);
  private t = inject(TranslationService);
  private toastr = inject(ToastrService);
  private currentUser = inject(CurrentUserService);
  private fb = inject(FormBuilder);
  private cityService = inject(CityService);

  group: GarbageGroup | null = null;
  loading = false;
  error: string | null = null;
  warn: string | null = null;
  GarbageGroupRole = GarbageGroupRole;
  actLoading = false;
  editDetails = false;
  savingDetails = false;
  cities: string[] = this.cityService.cities() ?? [];
  citiesLoading = false;
  citiesLoadError = false;

  private addressGroup: FormGroup = buildAddressFormGroup(this.fb);
  groupForm: FormGroup = this.fb.group({
    groupName: ['', [Validators.required, Validators.maxLength(100)]],
    groupDescription: ['', [Validators.required, Validators.maxLength(500)]],
    address: this.addressGroup
  });

  constructor() {
    const cityControl = this.addressGroup.get('city');
    const postalControl = this.addressGroup.get('postalCode');
    const streetControl = this.addressGroup.get('street');

    cityControl?.addValidators(Validators.maxLength(100));
    cityControl?.updateValueAndValidity({ emitEvent: false });

    postalControl?.addValidators(Validators.maxLength(12));
    postalControl?.updateValueAndValidity({ emitEvent: false });

    streetControl?.addValidators(Validators.maxLength(200));
    streetControl?.updateValueAndValidity({ emitEvent: false });
  }

  ngOnInit(): void {
    // Prefer resolver-provided data for optimal UX
    this.route.data.subscribe(d => {
      const resolved: GarbageGroup | null | undefined = d['group'];
      if (resolved) {
        this.group = resolved;
        this.error = null;
        this.loading = false;
        this.syncFormWithGroup(resolved, true);
        return;
      }
      const id = this.route.snapshot.paramMap.get('groupId');
      // Prefill from navigation state (preview from list)
      const state: any = history?.state;
      const preview: GarbageGroupInfo | undefined = state?.group;
      if (preview && preview.id === id) {
        this.group = {
          id: preview.id,
          name: preview.name,
          description: '',
          users: [],
          address: { city: '', postalCode: '', street: '' }
        } as GarbageGroup;
        this.syncFormWithGroup(this.group, true);
      }
      if (!id) {
        this.error = this.t.translate('groups.details.invalidId');
        return;
      }
      // Try to fetch; if backend not ready, component will show error but route stays open
      this.fetch(id);
    });
  }

  fetch(id: string) {
    this.loading = true;
    this.error = null;
    this.warn = null;
    this.groupService.details(id).subscribe({
      next: (res: { resultModel: GarbageGroup | null }) => {
        this.group = res.resultModel ?? null;
        this.loading = false;
        this.syncFormWithGroup(this.group);
      },
      error: (err: any) => {
        this.loading = false;
        // better diagnostics in UI
        const apiMsg = err?.error?.errorMessage;
        const code = err?.status ? ` (HTTP ${err.status})` : '';
        const defaultMsg = this.t.translate('groups.details.loadError');
        // If mamy choć częściowe dane (preview), pokazujemy łagodne ostrzeżenie zamiast bloku błędu
        if (this.group) {
          this.warn = apiMsg || `${defaultMsg}${code}`;
        } else {
          this.error = apiMsg || `${defaultMsg}${code}`;
        }
        try { console.error('Group details load failed', { id, err }); } catch {}
      }
    });
  }

  retry() {
    const id = this.route.snapshot.paramMap.get('groupId');
    if (!id) return;
    this.fetch(id);
  }

  isOwner(): boolean {
    // prefer resolver data (full users[]), fallback to no-owner to be safe
    const me = this.currentUser.user();
    if (!this.group || !this.group.users || !me) return false;
    const mine = this.group.users.find(u => u.id === me.id);
    return !!mine && mine.garbageGroupRole === GarbageGroupRole.Owner;
  }

  invite(ev: Event, userName: string) {
    ev.preventDefault();
    if (!userName || !this.group) return;
    this.actLoading = true;
    this.warn = null;
    this.groupService.inviteUser(this.group.id, userName).subscribe({
      next: () => {
        this.toastr.success(this.t.translate('groups.details.invite.success'));
        this.refreshDetails();
      },
      error: (err) => {
        this.warn = this.t.translate('groups.details.invite.error');
        this.actLoading = false;
        try { console.error('Invite user failed', { id: this.group?.id, userName, err }); } catch {}
      }
    });
  }

  remove(userId: string) {
    if (!this.group) return;
    this.actLoading = true;
    this.warn = null;
    this.groupService.removeUser(this.group.id, userId)
      .pipe(finalize(() => this.actLoading = false))
      .subscribe({
      next: () => {
        this.toastr.success(this.t.translate('groups.details.remove.success'));
        this.refreshDetails();
      }
    });
  }

  private refreshDetails(syncForm = false) {
    const id = this.group?.id || this.route.snapshot.paramMap.get('groupId');
    if (!id) { this.actLoading = false; return; }
    this.groupService.details(id).subscribe({
      next: (res: { resultModel: GarbageGroup | null }) => {
        this.group = res.resultModel ?? this.group;
        this.actLoading = false;
        this.syncFormWithGroup(this.group, syncForm);
      },
      error: () => {
        this.actLoading = false;
      }
    });
  }

  getGroupCity(): string {
    if (!this.group) return '';
    if (this.group.address.city) return this.group.address.city;
    return this.group.city ?? '';
  }

  getGroupPostalCode(): string {
    if (!this.group) return '';
    if (this.group.address.postalCode) return this.group.address.postalCode;
    return this.group.postalCode ?? '';
  }

  getGroupStreet(): string {
    if (!this.group) return '';
    return this.group.address.street;
  }

  startEdit(): void {
    if (!this.group) return;
    this.editDetails = true;
    this.savingDetails = false;
    this.warn = null;
    this.syncFormWithGroup(this.group, true);
    this.groupForm.markAsPristine();
  }

  cancelEdit(): void {
    this.editDetails = false;
    this.savingDetails = false;
    this.syncFormWithGroup(this.group, true);
  }

  saveGroup(): void {
    if (!this.group) return;
    if (this.groupForm.invalid) {
      this.groupForm.markAllAsTouched();
      return;
    }

    const formValue = this.groupForm.getRawValue() as {
      groupName: string;
      groupDescription: string;
      address: { city: string; postalCode: string; street: string; };
    };

    const payload: UpdateGarbageGroupRequest = {
      groupName: formValue.groupName.trim(),
      groupDescription: formValue.groupDescription.trim(),
      address: {
        city: formValue.address.city.trim(),
        postalCode: formValue.address.postalCode.trim(),
        street: formValue.address.street.trim()
      }
    };

    this.savingDetails = true;

    this.groupService.update(this.group.id, payload).subscribe({
      next: () => {
        this.toastr.success(this.t.translate('success.update'));
        this.savingDetails = false;
        this.editDetails = false;
        this.refreshDetails(true);
      },
      error: (err) => {
        this.savingDetails = false;
        const msg = err?.error?.errorMessage || this.t.translate('groups.details.updateError');
        this.warn = msg;
        this.toastr.error(msg);
        try { console.error('Group update failed', { id: this.group?.id, err }); } catch {}
      }
    });
  }

  hasControlError(path: string, error: string): boolean {
    const control = this.groupForm.get(path);
    if (!control) return false;
    return control.touched && control.hasError(error);
  }

  private syncFormWithGroup(group: GarbageGroup | null, force = false): void {
    if (!group) return;
    if (!force && this.editDetails) return;

    const city = group.address?.city || group.city || '';
    const postalCode = group.address?.postalCode || group.postalCode || '';
    const street = group.address?.street || '';

    this.groupForm.reset({
      groupName: group.name ?? '',
      groupDescription: group.description ?? '',
      address: {
        city,
        postalCode,
        street
      }
    });
    this.groupForm.markAsPristine();
  }
}
