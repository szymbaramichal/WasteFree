import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { TranslatePipe } from '../pipes/translate.pipe';
import { GarbageGroup, GarbageGroupInfo, GarbageGroupRole } from '../_models/garbageGroups';
import { GarbageGroupService } from '../services/garbage-group.service';
import { TranslationService } from '../services/translation.service';
import { ToastrService } from 'ngx-toastr';
import { CurrentUserService } from '../services/current-user.service';

@Component({
  selector: 'app-group-panel',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslatePipe],
  templateUrl: './group-panel.component.html',
  styleUrls: ['./group-panel.component.css']
})
export class GroupPanelComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private groupService = inject(GarbageGroupService);
  private t = inject(TranslationService);
  private toastr = inject(ToastrService);
  private currentUser = inject(CurrentUserService);

  group: GarbageGroup | null = null;
  loading = false;
  error: string | null = null;
  warn: string | null = null;
  GarbageGroupRole = GarbageGroupRole;
  actLoading = false;

  ngOnInit(): void {
    // Prefer resolver-provided data for optimal UX
    this.route.data.subscribe(d => {
      const resolved: GarbageGroup | null | undefined = d['group'];
      if (resolved) {
        this.group = resolved;
        this.error = null;
        this.loading = false;
        return;
      }
      const id = this.route.snapshot.paramMap.get('groupId');
      // Prefill from navigation state (preview from list)
      const state: any = history?.state;
      const preview: GarbageGroupInfo | undefined = state?.group;
      if (preview && preview.id === id) {
        this.group = { id: preview.id, name: preview.name, description: '', users: [] } as GarbageGroup;
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
    this.groupService.removeUser(this.group.id, userId).subscribe({
      next: () => {
        this.toastr.success(this.t.translate('groups.details.remove.success'));
        this.refreshDetails();
      },
      error: (err) => {
        this.warn = this.t.translate('groups.details.remove.error');
        this.actLoading = false;
        try { console.error('Remove user failed', { id: this.group?.id, userId, err }); } catch {}
      }
    });
  }

  private refreshDetails() {
    const id = this.group?.id || this.route.snapshot.paramMap.get('groupId');
    if (!id) { this.actLoading = false; return; }
    this.groupService.details(id).subscribe({
      next: (res: { resultModel: GarbageGroup | null }) => {
        this.group = res.resultModel ?? this.group;
        this.actLoading = false;
      },
      error: () => {
        // keep current view; action completed, but refresh failed
        this.actLoading = false;
      }
    });
  }
}
