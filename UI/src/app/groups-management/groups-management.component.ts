import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { GarbageGroupInfo, RegisterGarbageGroupRequest } from '../_models/garbageGroups';
import { GarbageGroupService } from '../services/garbage-group.service';
import { HttpClientModule } from '@angular/common/http';
import { finalize } from 'rxjs';
import { TranslatePipe } from '../pipes/translate.pipe';
import { ToastrService } from 'ngx-toastr';
import { TranslationService } from '../services/translation.service';

@Component({
  selector: 'app-groups-management',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, HttpClientModule, TranslatePipe],
  templateUrl: './groups-management.component.html',
  styleUrls: ['./groups-management.component.css']
})
export class GroupsManagementComponent {
  private fb = inject(FormBuilder);
  private groupService = inject(GarbageGroupService);
  private toastr = inject(ToastrService);
  private translationService = inject(TranslationService);

  groups: GarbageGroupInfo[] = [];
  loading = false;
  submitting = false;
  loadError: string | null = null;

  form: FormGroup = this.fb.group({
    groupName: ['', [Validators.required, Validators.maxLength(100)]],
    groupDescription: ['', [Validators.required, Validators.maxLength(500)]]
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.submitting = true;
    const payload: RegisterGarbageGroupRequest = this.form.value;
    this.groupService.register(payload)
      .pipe(finalize(() => this.submitting = false))
      .subscribe({
        next: () => {
          this.toastr.success(this.translationService.translate('success.update'));
          this.form.reset();
        }
      });
  }

  hasError(control: string, error: string): boolean {
    const c = this.form.get(control);
    return !!c && c.touched && c.hasError(error);
  }

  avatarColor(name: string): string {
    if(!name) return '#6c757d';
    const hash = Array.from(name).reduce((acc, ch) => acc + ch.charCodeAt(0), 0);
    const colors = ['#2bb673', '#1f8b56', '#198754', '#0d6efd', '#20c997', '#6f42c1', '#fd7e14'];
    return colors[hash % colors.length];
  }
}
