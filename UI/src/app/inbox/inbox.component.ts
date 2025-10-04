import { Component, signal, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { InboxService, NotificationItem } from '../services/inbox.service';
import { TranslatePipe } from '../pipes/translate.pipe';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-inbox',
  standalone: true,
  imports: [CommonModule, TranslatePipe, RouterModule],
  templateUrl: './inbox.component.html',
  styleUrl: './inbox.component.css'
})
export class InboxComponent {
  private inbox = inject(InboxService);
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

  relativeDate(date?: string) {
    if (!date) return '';
    const d = new Date(date).getTime();
    const diffMs = Date.now() - d;
    const mins = Math.floor(diffMs / 60000);
    if (mins < 1) return 'now';
    if (mins < 60) return mins + 'm';
    const hrs = Math.floor(mins / 60);
    if (hrs < 24) return hrs + 'h';
    const days = Math.floor(hrs / 24);
    return days + 'd';
  }

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
}
