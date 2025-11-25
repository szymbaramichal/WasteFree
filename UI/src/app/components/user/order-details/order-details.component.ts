import { CommonModule } from '@angular/common';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { TranslatePipe } from '@app/pipes/translate.pipe';
import { GarbageOrderDetailsDto, GarbageOrderDto, GarbageOrderStatus, GarbageOrderUserDto, PickupOption } from '@app/_models/garbage-orders';
import { GarbageOrderService, USER_ORDERS_PAGE_SIZE } from '@app/services/garbage-order.service';
import { TranslationService } from '@app/services/translation.service';
import { CurrentUserService } from '@app/services/current-user.service';
import { WalletService } from '@app/services/wallet.service';
import { ToastrService } from 'ngx-toastr';
import { finalize, map } from 'rxjs/operators';

@Component({
  selector: 'app-order-details',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslatePipe],
  templateUrl: './order-details.component.html',
  styleUrls: ['./order-details.component.css']
})
export class OrderDetailsComponent {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private orderService = inject(GarbageOrderService);
  private translation = inject(TranslationService);
  private currentUser = inject(CurrentUserService).user;
  private wallet = inject(WalletService);
  private toastr = inject(ToastrService);
  private destroyRef = inject(DestroyRef);

  readonly currentLocale = toSignal(
    this.translation.onLangChange.pipe(
      map(lang => lang === 'pl' ? 'pl' : 'en-US')
    ),
    { initialValue: 'en-US' }
  );

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly paying = signal(false);
  readonly utilizationPaying = signal(false);
  readonly order = signal<GarbageOrderDto | null>(null);
  readonly orderStatus = GarbageOrderStatus;
  readonly assignedAdminAvatar = signal<string | null>(null);
  readonly assignedAdminAvatarLoading = signal(false);
  readonly participantAvatars = signal<Record<string, string>>({});

  private lastOrderDetailsKey: string | null = null;
  private requestedOrderDetailsKey: string | null = null;

  constructor() {
    this.route.paramMap
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((params) => {
        const orderId = params.get('orderId');
        if (!orderId) {
          this.navigateBack();
          return;
        }
        this.resolveOrder(orderId);
      });

  }

  navigateBack(): void {
    this.router.navigate(['/portal/my-pickups']);
  }

  statusClass(status: GarbageOrderStatus): string {
    switch (status) {
      case GarbageOrderStatus.Completed:
        return 'status-chip status-completed';
      case GarbageOrderStatus.WaitingForPickup:
      case GarbageOrderStatus.WaitingForAccept:
      case GarbageOrderStatus.WaitingForPayment:
      case GarbageOrderStatus.WaitingForUtilizationFee:
        return 'status-chip status-scheduled';
      default:
        return 'status-chip status-pending';
    }
  }

  statusTranslationKey(status: GarbageOrderStatus): string {
    switch (status) {
      case GarbageOrderStatus.WaitingForPayment:
        return 'myPickups.status.waitingForPayment';
      case GarbageOrderStatus.WaitingForAccept:
        return 'myPickups.status.waitingForAccept';
      case GarbageOrderStatus.WaitingForPickup:
        return 'myPickups.status.waitingForPickup';
      case GarbageOrderStatus.WaitingForUtilizationFee:
        return 'myPickups.status.waitingForUtilizationFee';
      case GarbageOrderStatus.Completed:
        return 'myPickups.status.completed';
      case GarbageOrderStatus.Complained:
        return 'myPickups.status.complained';
      case GarbageOrderStatus.Resolved:
        return 'myPickups.status.resolved';
      case GarbageOrderStatus.Cancelled:
        return 'myPickups.status.cancelled';
      default:
        return 'myPickups.status.waitingForPickup';
    }
  }

  pickupOptionKey(option: PickupOption): string {
    switch (option) {
      case PickupOption.SmallPickup:
        return 'myPickups.option.smallPickup';
      case PickupOption.Pickup:
        return 'myPickups.option.pickup';
      case PickupOption.Container:
        return 'myPickups.option.container';
      case PickupOption.SpecialOrder:
        return 'myPickups.option.specialOrder';
      default:
        return 'myPickups.option.pickup';
    }
  }

  orderCode(pickup: GarbageOrderDto): string {
    return pickup.id?.slice(0, 8)?.toUpperCase() ?? pickup.id;
  }

  canPay(): boolean {
    const detail = this.order();
    if (!detail) {
      return false;
    }
    if (detail.garbageOrderStatus !== GarbageOrderStatus.WaitingForPayment) {
      return false;
    }
    const entry = this.currentUserEntry(detail);
    return !!entry && !entry.hasAcceptedPayment;
  }

  hasAcceptedPayment(): boolean {
    const detail = this.order();
    const entry = detail ? this.currentUserEntry(detail) : null;
    return entry?.hasAcceptedPayment ?? false;
  }

  pickupShareAmount(): number {
    const detail = this.order();
    const entry = detail ? this.currentUserEntry(detail) : null;
    return entry?.shareAmount ?? 0;
  }

  utilizationOutstandingAmount(): number {
    const detail = this.order();
    return detail?.additionalUtilizationFeeAmount ?? 0;
  }

  utilizationShareAmount(): number {
    const detail = this.order();
    const entry = detail ? this.currentUserEntry(detail) : null;
    return entry?.additionalUtilizationFeeShareAmount ?? 0;
  }

  hasPaidUtilizationFee(): boolean {
    const detail = this.order();
    const entry = detail ? this.currentUserEntry(detail) : null;
    return entry?.hasPaidAdditionalUtilizationFee ?? false;
  }

  hasUtilizationFeeShares(detail: GarbageOrderDto | null = null): boolean {
    const pickup = detail ?? this.order();
    if (!pickup) {
      return false;
    }
    if ((pickup.additionalUtilizationFeeAmount ?? 0) > 0) {
      return true;
    }
    return pickup.users.some(user => (user.additionalUtilizationFeeShareAmount ?? 0) > 0);
  }

  userHasUtilizationFeeShare(user: GarbageOrderUserDto): boolean {
    return (user.additionalUtilizationFeeShareAmount ?? 0) > 0;
  }

  canPayUtilizationFee(): boolean {
    const detail = this.order();
    if (!detail || detail.garbageOrderStatus !== GarbageOrderStatus.WaitingForUtilizationFee) {
      return false;
    }
    const entry = this.currentUserEntry(detail);
    if (!entry) {
      return false;
    }

    const share = entry.additionalUtilizationFeeShareAmount ?? 0;
    return share > 0 && !entry.hasPaidAdditionalUtilizationFee;
  }

  payForUtilizationFee(): void {
    const detail = this.order();
    if (!detail || this.utilizationPaying()) {
      return;
    }

    const entry = this.currentUserEntry(detail);
    if (!entry) {
      this.toastr.error(this.translation.translate('myPickups.details.utilization.shareError'));
      return;
    }

    const shareAmount = entry.additionalUtilizationFeeShareAmount ?? 0;
    if (shareAmount <= 0) {
      this.toastr.info(this.translation.translate('myPickups.details.utilization.noShare'));
      return;
    }

    this.utilizationPaying.set(true);

    this.orderService.payAdditionalUtilizationFee(detail.garbageGroupId, detail.id)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.utilizationPaying.set(false))
      )
      .subscribe({
        next: (res) => {
          const updated = res.resultModel ?? null;
          if (!updated) {
            return;
          }

          this.setOrderDetail(updated, true);
          this.wallet.adjustBalance(-shareAmount);
          void this.wallet.refreshBalance();
          this.toastr.success(this.translation.translate('myPickups.details.utilization.paySuccess'));
        }
      });
  }

  payForOrder(): void {
    const detail = this.order();
    if (!detail || this.paying()) {
      return;
    }
    const entry = this.currentUserEntry(detail);
    if (!entry) {
      return;
    }

    this.paying.set(true);
    const shareAmount = entry.shareAmount;

    this.orderService.payForOrder(detail.garbageGroupId, detail.id)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.paying.set(false))
      )
      .subscribe((res) => {
        const updated = res.resultModel ?? null;
        if (updated) {
          this.setOrderDetail(updated, true);
        }
        if (shareAmount > 0) {
          this.wallet.adjustBalance(-shareAmount);
        }
        void this.wallet.refreshBalance();
        this.toastr.success(this.translation.translate('myPickups.details.paySuccess'));
      });
  }

  private resolveOrder(orderId: string): void {
    const cached = this.orderService.findOrderById(orderId);
    const showLoader = !cached;

    this.error.set(null);
    this.loading.set(showLoader);

    if (cached) {
      this.setOrderDetail(cached, true);
    } else {
      this.setOrderDetail(null, true);
    }

    this.orderService.getMyOrders(1, USER_ORDERS_PAGE_SIZE)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false))
      )
        .subscribe({
          next: (res) => {
            if (res.errorMessage) {
              if (!cached) {
                const message = res.errorMessage || this.translation.translate('myPickups.details.loadError');
                this.error.set(message);
                this.setOrderDetail(null, true);
              }
              return;
            }

            const refreshed = this.orderService.findOrderById(orderId);
            if (refreshed) {
              this.setOrderDetail(refreshed, true);
              return;
            }

            const message = this.translation.translate('myPickups.details.notFound');
            this.error.set(message);
            this.setOrderDetail(null, true);
          },
          error: () => {
            if (!cached) {
              const message = this.translation.translate('myPickups.details.loadError');
              this.error.set(message);
              this.setOrderDetail(null, true);
            }
          }
        });
  }

  assignedAdminDisplayName(detail: GarbageOrderDto | null): string {
    const fallback = this.translation.translate('myPickups.details.assignedAdmin.fallbackUsername');
    if (!detail?.assignedGarbageAdminId) {
      return fallback;
    }

    const raw = detail.assignedGarbageAdminUsername;
    if (typeof raw !== 'string') {
      return fallback;
    }

    const trimmed = raw.trim();
    return trimmed.length > 0 ? trimmed : fallback;
  }

  assignedAdminInitials(detail: GarbageOrderDto | null): string {
    const name = this.assignedAdminDisplayName(detail);
    const tokens = name.split(/\s+/).filter(token => token.length > 0);
    if (tokens.length === 0) {
      return '?';
    }

    const letters = tokens.slice(0, 2).map(token => token.charAt(0).toUpperCase()).join('');
    if (letters.length > 0) {
      return letters;
    }

    return name.slice(0, 2).toUpperCase();
  }

  assignedAdminAvatarAlt(detail: GarbageOrderDto | null): string {
    return this.firstNicknameLetter(this.assignedAdminDisplayName(detail));
  }

  participantDisplayName(user: GarbageOrderUserDto): string {
    const raw = typeof user.username === 'string' ? user.username.trim() : '';
    return raw.length > 0 ? raw : user.userId;
  }

  participantInitials(user: GarbageOrderUserDto): string {
    const name = this.participantDisplayName(user);
    const tokens = name.split(/\s+/).filter(Boolean);
    if (tokens.length === 0) {
      return user.userId.slice(0, 2).toUpperCase();
    }

    const letters = tokens.slice(0, 2).map(token => token.charAt(0).toUpperCase()).join('');
    return letters.length > 0 ? letters : user.userId.slice(0, 2).toUpperCase();
  }

  participantAvatarAlt(user: GarbageOrderUserDto): string {
    return this.firstNicknameLetter(this.participantDisplayName(user));
  }

  participantAvatarUrl(userId: string | null | undefined): string | null {
    if (!userId) {
      return null;
    }

    const normalizedKey = userId.toLowerCase();
    const map = this.participantAvatars();
    return map[normalizedKey] ?? map[userId] ?? null;
  }

  private loadOrderAvatars(detail: GarbageOrderDto | null, forceRefresh = false): void {
    if (!detail) {
      this.assignedAdminAvatar.set(null);
      this.participantAvatars.set({});
      this.assignedAdminAvatarLoading.set(false);
      this.lastOrderDetailsKey = null;
      this.requestedOrderDetailsKey = null;
      return;
    }

    const key = this.buildOrderDetailsKey(detail);
    if (!forceRefresh) {
      if (key && key === this.lastOrderDetailsKey) {
        return;
      }
      if (key && key === this.requestedOrderDetailsKey) {
        return;
      }
    }

    if (key) {
      this.requestedOrderDetailsKey = key;
    }

    this.assignedAdminAvatarLoading.set(true);

    this.orderService.getGarbageOrderDetails(detail.id)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          if (!key || this.requestedOrderDetailsKey === key) {
            this.requestedOrderDetailsKey = null;
          }
          this.assignedAdminAvatarLoading.set(false);
        })
      )
      .subscribe({
        next: (res) => {
          if (!res || res.errorMessage || !res.resultModel) {
            this.handleOrderDetailsFailure();
            return;
          }

          const adminUrl = this.normalizeAvatar(res.resultModel.assignedAdminAvatarUrl);
          const finalAdminUrl = adminUrl ? this.applyAvatarCacheBuster(adminUrl) : null;
          this.assignedAdminAvatar.set(finalAdminUrl);
          this.participantAvatars.set(this.normalizeAvatarMap(res.resultModel.userAvatarsUrls));
          this.lastOrderDetailsKey = key;
        },
        error: () => {
          this.handleOrderDetailsFailure();
        }
      });
  }

  private handleOrderDetailsFailure(): void {
    this.assignedAdminAvatar.set(null);
    this.participantAvatars.set({});
    this.lastOrderDetailsKey = null;
  }

  private buildOrderDetailsKey(order: GarbageOrderDto | null): string | null {
    if (!order) {
      return null;
    }

    const adminPart = `${order.assignedGarbageAdminId ?? 'na'}:${order.assignedGarbageAdminAvatarName ?? 'na'}`;
    const participantsPart = order.users.map(user => user.userId).sort().join('|');
    return `${order.id}:${adminPart}:${participantsPart}`;
  }

  private normalizeAvatar(value: unknown): string | null {
    if (value === null || value === undefined) {
      return null;
    }

    let resolved: string;
    if (typeof value === 'string') {
      resolved = value;
    } else if (value instanceof URL) {
      resolved = value.toString();
    } else {
      resolved = String(value);
    }

    const trimmed = resolved.trim().replace(/^"+|"+$/g, '');
    return trimmed.length > 0 ? trimmed : null;
  }

  private normalizeAvatarMap(source: Record<string, string> | null | undefined): Record<string, string> {
    if (!source) {
      return {};
    }

    const normalized: Record<string, string> = {};
    Object.entries(source).forEach(([rawKey, rawValue]) => {
      const avatarUrl = this.normalizeAvatar(rawValue);
      if (!avatarUrl) {
        return;
      }

      const finalUrl = this.applyAvatarCacheBuster(avatarUrl);
      normalized[rawKey.toLowerCase()] = finalUrl;
    });

    return normalized;
  }

  private firstNicknameLetter(value: string | null | undefined): string {
    if (!value) {
      return '?';
    }

    const trimmed = value.trim();
    return trimmed.length > 0 ? trimmed.charAt(0).toUpperCase() : '?';
  }

  private applyAvatarCacheBuster(url: string): string {
    try {
      const parsed = new URL(url);
      parsed.searchParams.delete('v');
      parsed.searchParams.set('v', Date.now().toString());
      return parsed.toString();
    } catch {
      const separator = url.includes('?') ? '&' : '?';
      return `${url}${separator}v=${Date.now()}`;
    }
  }

  private setOrderDetail(detail: GarbageOrderDto | null, forceAvatarRefresh = false): void {
    this.order.set(detail);
    this.loadOrderAvatars(detail, forceAvatarRefresh);
  }

  private currentUserEntry(pickup: GarbageOrderDto): GarbageOrderUserDto | null {
    const me = this.currentUser();
    if (!me) {
      return null;
    }
    return pickup.users.find((user) => user.userId === me.id) ?? null;
  }
}
