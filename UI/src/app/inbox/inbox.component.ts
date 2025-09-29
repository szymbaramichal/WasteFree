import { Component, computed, signal, inject } from '@angular/core';
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
  // filter signals
  filter = signal<'all' | 'unread' | 'info' | 'warning' | 'success'>('all');
  now = new Date();

  notifications = this.inbox.notifications;
  loading = this.inbox.loading;
  error = this.inbox.error;

  filtered = computed(() => {
    const f = this.filter();
    const list = this.notifications();
    if (f === 'all') return list;
    if (f === 'unread') return list.filter(n => !n.read);
    return list.filter(n => n.type === f);
  });

  constructor() {
    if (!this.notifications().length) {
      this.inbox.fetchNotifications();
    }
  }

  setFilter(f: 'all' | 'unread' | 'info' | 'warning' | 'success') {
    this.filter.set(f);
  }

  refresh() { this.inbox.fetchNotifications(); }
  markAllRead() { this.inbox.markAllRead(); }
  markRead(n: NotificationItem) { if (!n.read) this.inbox.markAsRead(n.id); }

  anyUnread() { return this.notifications().some(n => !n.read); }
  unreadCount() { return this.notifications().filter(n => !n.read).length; }

  trackById(_i: number, n: NotificationItem) { return n.id; }
  cssForType(t: string) {
    switch (t) {
      case 'warning': return 'warning';
      case 'success': return 'success';
      case 'info': return 'info';
      default: return 'secondary';
    }
  }
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

  // Safely convert message payload to HTML string for rendering.
  // If message is an object, try common fields or fallback to JSON.
  getMessage(n: NotificationItem): string {
    const m: any = n.message as any;
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
