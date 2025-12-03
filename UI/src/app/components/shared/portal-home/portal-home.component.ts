import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslatePipe } from '@app/pipes/translate.pipe';
import { RouterModule } from '@angular/router';
import { InboxService } from '@app/services/inbox.service';
import { WalletService } from '@app/services/wallet.service';
import { CurrentUserService } from '@app/services/current-user.service';
import { ShowForRolesDirective } from '@app/directives/show-for-roles.directive';
import { UserRole } from '@app/_models/user';
import { GarbageOrderService, USER_ORDERS_PAGE_SIZE } from '@app/services/garbage-order.service';
import { GarbageGroupService } from '@app/services/garbage-group.service';
import { GarbageOrderStatus, PickupOption } from '@app/_models/garbage-orders';

@Component({
  selector: 'app-portal-home',
  standalone: true,
  imports: [CommonModule, TranslatePipe, RouterModule, ShowForRolesDirective],
  templateUrl: './portal-home.component.html',
  styleUrl: './portal-home.component.css'
})
export class PortalHomeComponent {
  inbox = inject(InboxService);
  private wallet = inject(WalletService);
  private orderService = inject(GarbageOrderService);
  private groupService = inject(GarbageGroupService);
  currentUser = inject(CurrentUserService).user;
  userRole = UserRole;

  stats = signal([
    { key: 'savings', labelKey: 'portal.home.stats.savings', value: 0, unit: 'PLN', icon: 'wallet' },
    { key: 'wasteReduced', labelKey: 'portal.home.stats.wasteReduced', value: 0, unit: 'kg', icon: 'leaf' },
    { key: 'collections', labelKey: 'portal.home.stats.collections', value: 0, unit: '', icon: 'truck' },
    { key: 'community', labelKey: 'portal.home.stats.community', value: 0, unit: '', icon: 'users' }
  ]);

  balance = 0;
  readonly notificationsExpanded = signal(false);
  
  recent = computed(() => {
    const all = this.inbox.notifications();
    const limit = this.notificationsExpanded() ? 5 : 2;
    return all.slice(0, limit);
  });

  hasMore = computed(() => this.inbox.notifications().length > 2);

  toggleNotifications() {
    this.notificationsExpanded.update(v => !v);
  }

  async ngOnInit() {
    this.inbox.refreshCounter();
    this.inbox.fetchNotifications(1, 5);
    this.wallet.balance$.subscribe(b => this.balance = b);
    this.loadStats();
    await this.wallet.ensureInit();
    this.balance = this.wallet.currentBalance;
  }

  private loadStats() {
    const user = this.currentUser();
    if (!user || user.role !== UserRole.User) {
      return;
    }

    this.orderService.getMyOrders(1, USER_ORDERS_PAGE_SIZE).subscribe(res => {
      const orders = res.resultModel || [];
      const completed = orders.filter(o => o.garbageOrderStatus === GarbageOrderStatus.Completed);

      const totalCost = completed.reduce((acc, o) => acc + o.cost, 0);
      const savings = totalCost * 0.2; // Assume 20% savings

      const wasteReduced = completed.reduce((acc, o) => acc + this.getWeight(o.pickupOption), 0);
      const collections = completed.length;

      this.stats.update(s => {
        const newStats = [...s];
        newStats[0].value = savings;
        newStats[1].value = wasteReduced;
        newStats[2].value = collections;
        return newStats;
      });
    });

    this.groupService.list().subscribe(res => {
      const groups = res.resultModel || [];
      const count = groups.length;
      this.stats.update(s => {
        const newStats = [...s];
        newStats[3].value = count;
        return newStats;
      });
    });
  }

  private getWeight(option: PickupOption): number {
    switch (option) {
      case PickupOption.SmallPickup: return 5;
      case PickupOption.Pickup: return 20;
      case PickupOption.Container: return 100;
      case PickupOption.SpecialOrder: return 50;
      default: return 0;
    }
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
