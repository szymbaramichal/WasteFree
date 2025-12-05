import { Component, signal, inject, computed } from '@angular/core';
import { take } from 'rxjs/operators';
import { CommonModule } from '@angular/common';
import { InboxService } from '@app/services/inbox.service';
import { TranslatePipe } from '@app/pipes/translate.pipe';
import { Router, RouterModule } from '@angular/router';
import { InboxActionType, NotificationItem } from '@app/_models/inbox';
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
  private router = inject(Router);
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
    if (!this.canRespond(notification)) return;
    this.inbox.makeAction(notification.id, true)
      .pipe(take(1))
      .subscribe({ next: () => {
        this.refresh();
        this.toastr.success(this.translateService.translate('success.update'));
        this.inbox.refreshCounter();
      } });
  }

  decline(notification: NotificationItem) {
    if (!this.canRespond(notification)) return;
    this.inbox.makeAction(notification.id, false)
      .pipe(take(1))
      .subscribe({ next: () => { 
        this.refresh()
        this.toastr.success(this.translateService.translate('success.update'));
        this.inbox.refreshCounter();
      } });
  }

  canRespond(notification: NotificationItem): boolean {
    return this.resolveActionType(notification) === InboxActionType.GroupInvitation;
  }

  hasRedirect(notification: NotificationItem): boolean {
    return this.getRedirectRoute(notification) !== null;
  }

  goToTarget(notification: NotificationItem) {
    const route = this.getRedirectRoute(notification);
    if (!route) return;
    this.router.navigate(route).catch(() => {
      this.toastr.error(this.translateService.translate('error.generic'));
    });
  }

  private getRedirectRoute(notification: NotificationItem): string[] | null {
    const type = this.resolveActionType(notification);
    switch (type) {
      case InboxActionType.GroupInvitation:
        return ['/portal/groups'];
      case InboxActionType.MakePayment:
        return this.orderRoute(notification);
      case InboxActionType.GarbageOrderDetails:
        return this.orderRoute(notification);
      default:
        return null;
    }
  }

  private orderRoute(notification: NotificationItem): string[] {
    return notification.relatedEntityId
      ? ['/portal/my-pickups', notification.relatedEntityId]
      : ['/portal/my-pickups'];
  }

  private resolveActionType(notification: NotificationItem): InboxActionType {
    const raw = notification.actionType;
    if (raw === null || raw === undefined) {
      return InboxActionType.None;
    }
    if (typeof raw === 'number') {
      return this.isKnownActionType(raw) ? raw as InboxActionType : InboxActionType.None;
    }

    const asNumber = Number(raw);
    if (!Number.isNaN(asNumber) && this.isKnownActionType(asNumber)) {
      return asNumber as InboxActionType;
    }

    if (typeof raw === 'string') {
      const enumValue = (InboxActionType as unknown as Record<string, InboxActionType>)[raw];
      if (typeof enumValue === 'number') {
        return enumValue;
      }
    }
    return InboxActionType.None;
  }

  private isKnownActionType(value: number): value is InboxActionType {
    return value === InboxActionType.None
      || value === InboxActionType.GroupInvitation
      || value === InboxActionType.MakePayment
      || value === InboxActionType.GarbageOrderDetails;
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
