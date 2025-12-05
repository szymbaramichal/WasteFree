import { Component, signal, inject, computed } from '@angular/core';
import { take } from 'rxjs/operators';
import { CommonModule } from '@angular/common';
import { InboxService } from '@app/services/inbox.service';
import { TranslatePipe } from '@app/pipes/translate.pipe';
import { RouterModule } from '@angular/router';
import { NotificationItem } from '@app/_models/inbox';
import { ToastrService } from 'ngx-toastr';
import { TranslationService } from '@app/services/translation.service';

@Component({
  selector: 'app-inbox',
  standalone: true,
  imports: [CommonModule, TranslatePipe, RouterModule],
  templateUrl: './inbox.component.html',
  styleUrl: './inbox.component.css'
})
export class InboxComponent {
  private inbox = inject(InboxService);
  private toastr = inject(ToastrService);
  private translateService = inject(TranslationService);
  now = new Date();

  notifications = this.inbox.notifications;
  loading = this.inbox.loading;
  error = this.inbox.error;
  pager = this.inbox.pager;

  // Generic pagination state
  pageNumber = signal(1);
  pageSize = signal(10);

  totalPages = computed(() => this.pager()?.totalPages ?? 1);
  totalCount = computed(() => this.pager()?.totalCount ?? 0);

  constructor() {
    this.refresh();
  }

  refresh() { this.inbox.fetchNotifications(this.pageNumber(), this.pageSize()); }

  nextPage() { if (this.pageNumber() < (this.totalPages() || 1)) { this.pageNumber.update(p => p + 1); this.refresh(); } }
  prevPage() { if (this.pageNumber() > 1) { this.pageNumber.update(p => p - 1); this.refresh(); } }
  setPageSize(size: number) { this.pageSize.set(size); this.pageNumber.set(1); this.refresh(); }

  trackById(_i: number, n: NotificationItem) { return n.id; }

  getMessage(n: NotificationItem): string {
    const m: any = n.body as any;
    if (!m && m !== 0) return '';
    if (typeof m === 'string') return m;
    if (typeof m === 'object') {
      if (m.message && typeof m.message === 'string') return m.message;
      if (m.text && typeof m.text === 'string') return m.text;
      // fallback: stringify but compact
      try { return JSON.stringify(m); } catch { return String(m); }
    }
    return String(m);
  }

  formatLocalDate(date: string): string {
    const hasZone = /[zZ]|[+\-]\d{2}:?\d{2}$/.test(date);
    const normalized = hasZone ? date : `${date}Z`;
    const value = new Date(normalized);
    if (Number.isNaN(value.getTime())) return '';
    return value.toLocaleString(this.translateService.currentLang, {
      day: '2-digit',
      month: 'short',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  accept(notification: NotificationItem) {
    this.inbox.makeAction(notification.id, true)
      .pipe(take(1))
      .subscribe({ next: () => {
        this.refresh();
        this.toastr.success(this.translateService.translate('success.update'));
      } });
  }

  decline(notification: NotificationItem) {
    this.inbox.makeAction(notification.id, false)
      .pipe(take(1))
      .subscribe({ next: () => { 
        this.refresh()
        this.toastr.success(this.translateService.translate('success.update'));
      } });
  }

  dismiss(notification: NotificationItem) {
    this.inbox.deleteMessage(notification.id)
      .pipe(take(1))
      .subscribe({ next: () => this.refresh() });
  }

  clearAll() {
    this.inbox.clearAll()
      .pipe(take(1))
      .subscribe({ next: () => {
        this.pageNumber.set(1);
        this.refresh();
        this.toastr.success(this.translateService.translate('success.update'));
      } });
  }
}
