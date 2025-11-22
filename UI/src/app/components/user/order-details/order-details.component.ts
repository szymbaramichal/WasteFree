import { CommonModule } from '@angular/common';
import { Component, DestroyRef, effect, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { TranslatePipe } from '@app/pipes/translate.pipe';
import { GarbageOrderDto, GarbageOrderStatus, GarbageOrderUserDto, PickupOption } from '@app/_models/garbage-orders';
import { GarbageOrderService, USER_ORDERS_PAGE_SIZE } from '@app/services/garbage-order.service';
import { TranslationService } from '@app/services/translation.service';
import { CurrentUserService } from '@app/services/current-user.service';
import { WalletService } from '@app/services/wallet.service';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs/operators';

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

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly paying = signal(false);
  readonly utilizationPaying = signal(false);
  readonly order = signal<GarbageOrderDto | null>(null);
  readonly orderStatus = GarbageOrderStatus;
  readonly assignedAdminAvatar = signal<string | null>(null);
  readonly assignedAdminAvatarLoading = signal(false);

  private lastAssignedAdminAvatarKey: string | null = null;
  private requestedAssignedAdminAvatarKey: string | null = null;

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

    effect(() => {
      const current = this.order();
      this.handleOrderChange(current);
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

          this.order.set(updated);
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
          this.order.set(updated);
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
      this.order.set(cached);
    } else {
      this.order.set(null);
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
                this.order.set(null);
              }
              return;
            }

            const refreshed = this.orderService.findOrderById(orderId);
            if (refreshed) {
              this.order.set(refreshed);
              return;
            }

            const message = this.translation.translate('myPickups.details.notFound');
            this.error.set(message);
            this.order.set(null);
          },
          error: () => {
            if (!cached) {
              const message = this.translation.translate('myPickups.details.loadError');
              this.error.set(message);
              this.order.set(null);
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
    const template = this.translation.translate('myPickups.details.assignedAdmin.avatarAlt');
    const username = this.assignedAdminDisplayName(detail);
    return template.replace(/\{\{\s*username\s*\}\}|\{\s*username\s*\}/g, username);
  }

  private handleOrderChange(detail: GarbageOrderDto | null): void {
    this.loadAssignedGarbageAdminAvatar(detail);
  }

  private loadAssignedGarbageAdminAvatar(detail: GarbageOrderDto | null, forceRefresh = false): void {
    if (!detail?.assignedGarbageAdminId) {
      this.assignedAdminAvatar.set(null);
      this.assignedAdminAvatarLoading.set(false);
      this.lastAssignedAdminAvatarKey = null;
      this.requestedAssignedAdminAvatarKey = null;
      return;
    }

    const key = this.buildAssignedAdminAvatarKey(detail);
    if (!forceRefresh) {
      if (key && key === this.lastAssignedAdminAvatarKey) {
        return;
      }
      if (key && key === this.requestedAssignedAdminAvatarKey) {
        return;
      }
    }

    if (key) {
      this.requestedAssignedAdminAvatarKey = key;
    }

    this.assignedAdminAvatarLoading.set(true);

    this.orderService.getAssignedGarbageAdminAvatar(detail.id)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          if (!key || this.requestedAssignedAdminAvatarKey === key) {
            this.requestedAssignedAdminAvatarKey = null;
          }
          this.assignedAdminAvatarLoading.set(false);
        })
      )
      .subscribe({
        next: (res) => {
          if (!res || res.errorMessage) {
            this.assignedAdminAvatar.set(null);
            this.lastAssignedAdminAvatarKey = forceRefresh ? null : key;
            return;
          }

          const rawModel = res.resultModel ?? (res as any)?.resultModel ?? null;
          const rawUrl = rawModel?.avatarUrl ?? rawModel?.AvatarUrl ?? rawModel ?? null;
          const url = this.normalizeAvatar(rawUrl);
          this.assignedAdminAvatar.set(url);
          this.lastAssignedAdminAvatarKey = url ? key : null;
        },
        error: () => {
          this.assignedAdminAvatar.set(null);
          if (this.lastAssignedAdminAvatarKey === key) {
            this.lastAssignedAdminAvatarKey = null;
          }
        }
      });
  }

  private buildAssignedAdminAvatarKey(order: GarbageOrderDto | null): string | null {
    if (!order?.assignedGarbageAdminId) {
      return null;
    }
    const avatarName = order.assignedGarbageAdminAvatarName ?? '';
    return `${order.id}:${order.assignedGarbageAdminId}:${avatarName}`;
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

  private currentUserEntry(pickup: GarbageOrderDto): GarbageOrderUserDto | null {
    const me = this.currentUser();
    if (!me) {
      return null;
    }
    return pickup.users.find((user) => user.userId === me.id) ?? null;
  }
}
