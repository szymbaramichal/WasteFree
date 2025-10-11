import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslatePipe } from '../pipes/translate.pipe';
import { RouterModule } from '@angular/router';
import { InboxService } from '../services/inbox.service';
import { WalletService } from '../services/wallet.service';
import { CurrentUserService } from '../services/current-user.service';

@Component({
  selector: 'app-portal-home',
  standalone: true,
  imports: [CommonModule, TranslatePipe, RouterModule],
  templateUrl: './portal-home.component.html',
  styleUrl: './portal-home.component.css'
})
export class PortalHomeComponent {
  inbox = inject(InboxService);
  private wallet = inject(WalletService);
  currentUser = inject(CurrentUserService).user;

  stats = signal([
    { key: 'savings', labelKey: 'portal.home.stats.savings', value: 1280, unit: 'PLN', icon: 'wallet' },
    { key: 'wasteReduced', labelKey: 'portal.home.stats.wasteReduced', value: 342, unit: 'kg', icon: 'leaf' },
    { key: 'collections', labelKey: 'portal.home.stats.collections', value: 24, unit: '', icon: 'truck' },
    { key: 'community', labelKey: 'portal.home.stats.community', value: 3, unit: '', icon: 'users' }
  ]);

  balance = 0;
  recent = computed(() => this.inbox.notifications().slice(0, 5));

  async ngOnInit() {
    this.inbox.refreshCounter();
    this.inbox.fetchNotifications(1, 5);
    this.wallet.balance$.subscribe(b => this.balance = b);
    await this.wallet.ensureInit();
    this.balance = this.wallet.currentBalance;
  }

  iconPath(name: string) {
    switch (name) {
      case 'wallet': return `<svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M2 7h20v10H2z"/><path d="M16 7V5a2 2 0 0 0-2-2H4"/><circle cx="18" cy="12" r="1"/></svg>`;
      case 'leaf': return `<svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M11 21C4 21 3 13 5 8s6-6 11-6c0 5-2 9-7 11 5 0 9-2 11-7 0 7-4 15-9 15z"/></svg>`;
      case 'truck': return `<svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M3 7h12v10H3z"/><path d="M15 10h4l2 3v4h-6z"/><circle cx="7.5" cy="17.5" r="1.5"/><circle cx="17.5" cy="17.5" r="1.5"/></svg>`;
      case 'users': return `<svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>`;
      default: return '';
    }
  }

  inboxTrack(_i: number, n: any) { return n.id; }
  notifTypeColor(t: string) {
    switch (t) {
      case 'warning': return 'warning';
      case 'success': return 'success';
      case 'info': return 'info';
      default: return 'secondary';
    }
  }
  formatLocalDate(date: string): string {
    const hasZone = /[zZ]|[+\-]\d{2}:?\d{2}$/.test(date);
    const normalized = hasZone ? date : `${date}Z`;
    const value = new Date(normalized);
    if (Number.isNaN(value.getTime())) return '';
    return value.toLocaleString(undefined, {
      day: '2-digit',
      month: 'short',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  formatMessage(m: any): string {
    if (!m && m !== 0) return '';
    if (typeof m === 'string') return m;
    if (typeof m === 'object') {
      if (m.message && typeof m.message === 'string') return m.message;
      if (m.text && typeof m.text === 'string') return m.text;
      try { return JSON.stringify(m); } catch { return String(m); }
    }
    return String(m);
  }
}
