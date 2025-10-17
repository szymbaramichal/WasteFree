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

  groups: GarbageGroupInfo[] = [];
  loading = false;
  submitting = false;
  loadError: string | null = null;
  invitationsLoading = false;
  invitationsError: string | null = null;
  pendingInvitations: GarbageGroupInvitation[] = [];
  invitationActions: Record<string, boolean> = {};

  form: FormGroup = this.fb.group({
    groupName: ['', [Validators.required, Validators.maxLength(100)]],
    groupDescription: ['', [Validators.required, Validators.maxLength(500)]],
    city: ['', [Validators.required, Validators.maxLength(100)]],
    postalCode: ['', [Validators.required, Validators.maxLength(12)]],
    address: ['', [Validators.required, Validators.maxLength(200)]]
  });

  ngOnInit(): void {
    this.fetchPendingInvitations();
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.submitting = true;
    const payload: RegisterGarbageGroupRequest = this.form.getRawValue();
    this.groupService.register(payload)
      .pipe(finalize(() => this.submitting = false))
      .subscribe({
        next: () => {
          this.toastr.success(this.translationService.translate('success.update'));
          this.form.reset({
            groupName: '',
            groupDescription: '',
            city: '',
            postalCode: '',
            address: ''
          });
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
}
