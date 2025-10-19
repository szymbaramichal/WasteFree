import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { GarbageGroupInfo, GarbageGroupInvitation, RegisterGarbageGroupRequest } from '@app/_models/garbageGroups';
import { GarbageGroupService } from '@app/services/garbage-group.service';
import { HttpClientModule } from '@angular/common/http';
import { finalize } from 'rxjs';
import { TranslatePipe } from '@app/pipes/translate.pipe';
import { ToastrService } from 'ngx-toastr';
import { TranslationService } from '@app/services/translation.service';
import { CityService } from '@app/services/city.service';
import { buildAddressFormGroup } from '@app/forms/address-form';
import { Address } from '@app/_models/address';

@Component({
  selector: 'app-groups-management',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, HttpClientModule, TranslatePipe],
  templateUrl: './groups-management.component.html',
  styleUrls: ['./groups-management.component.css']
})
export class GroupsManagementComponent implements OnInit {
  private fb = inject(FormBuilder);
  private groupService = inject(GarbageGroupService);
  private toastr = inject(ToastrService);
  private translationService = inject(TranslationService);
  private cityService = inject(CityService);

  groups: GarbageGroupInfo[] = [];
  loading = false;
  submitting = false;
  loadError: string | null = null;
  invitationsLoading = false;
  invitationsError: string | null = null;
  pendingInvitations: GarbageGroupInvitation[] = [];
  invitationActions: Record<string, boolean> = {};

  cities: string[] = this.cityService.cities() ?? [];

  private addressGroup: FormGroup = buildAddressFormGroup(this.fb);

  form: FormGroup = this.fb.group({
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
    this.fetchPendingInvitations();
    this.resetFormDefaults(false);
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.submitting = true;
    const raw = this.form.getRawValue() as { groupName: string; groupDescription: string; address: Address | undefined };
    const rawAddress = raw.address ?? { city: '', postalCode: '', street: '' };
    const payload: RegisterGarbageGroupRequest = {
      groupName: raw.groupName.trim(),
      groupDescription: raw.groupDescription.trim(),
      address: {
        city: rawAddress.city?.trim() ?? '',
        postalCode: rawAddress.postalCode?.trim() ?? '',
        street: rawAddress.street?.trim() ?? ''
      }
    };
    this.groupService.register(payload)
      .pipe(finalize(() => this.submitting = false))
      .subscribe({
        next: () => {
          this.toastr.success(this.translationService.translate('success.update'));
          this.resetFormDefaults();
          this.fetchPendingInvitations();
        }
      });
  }

  hasError(control: string, error: string): boolean {
    const c = this.form.get(control);
    return !!c && c.touched && c.hasError(error);
  }

  fetchPendingInvitations(): void {
    this.invitationsLoading = true;
    this.invitationsError = null;
    this.groupService.pendingInvitations()
      .pipe(finalize(() => this.invitationsLoading = false))
      .subscribe({
        next: (res) => {
          this.pendingInvitations = res.resultModel ?? [];
        }
      });
  }

  acceptInvitation(groupId: string): void {
    this.handleInvitation(groupId, true);
  }

  declineInvitation(groupId: string): void {
    this.handleInvitation(groupId, false);
  }

  private handleInvitation(groupId: string, accept: boolean): void {
    if (!groupId || this.invitationActions[groupId]) {
      return;
    }
    this.setInvitationAction(groupId, true);
    this.groupService.respondToInvitation(groupId, accept)
      .pipe(finalize(() => this.setInvitationAction(groupId, false)))
      .subscribe({
        next: () => {
          this.pendingInvitations = this.pendingInvitations.filter(inv => inv.groupId !== groupId);
          const key = accept ? 'groups.invitations.accepted' : 'groups.invitations.declined';
          this.toastr.success(this.translationService.translate(key));
        }
      });
  }

  private setInvitationAction(groupId: string, loading: boolean): void {
    this.invitationActions = { ...this.invitationActions, [groupId]: loading };
  }

  isInvitationBusy(groupId: string): boolean {
    return !!this.invitationActions[groupId];
  }

  resetFormDefaults(preserveCity: boolean = true): void {
    const cityControl = this.addressGroup.get('city');
    const currentCity = cityControl?.value ?? '';
    const defaultCity = preserveCity ? (currentCity || this.cities[0] || '') : (this.cities[0] || '');

    this.form.reset({
      groupName: '',
      groupDescription: '',
      address: {
        city: defaultCity,
        postalCode: '',
        street: ''
      }
    });
  }
}
