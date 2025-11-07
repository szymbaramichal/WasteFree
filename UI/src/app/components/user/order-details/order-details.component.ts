import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { TranslatePipe } from '@app/pipes/translate.pipe';
import { GarbageOrderDto, GarbageOrderStatus, PickupOption } from '@app/_models/garbage-orders';
import { GarbageOrderService, USER_ORDERS_PAGE_SIZE } from '@app/services/garbage-order.service';
import { TranslationService } from '@app/services/translation.service';
import { CurrentUserService } from '@app/services/current-user.service';
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
  private toastr = inject(ToastrService);

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly paying = signal(false);
  readonly order = signal<GarbageOrderDto | null>(null);
  readonly orderStatus = GarbageOrderStatus;

  constructor() {
    this.route.paramMap
      .pipe(takeUntilDestroyed())
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

  payForOrder(): void {
    const detail = this.order();
    if (!detail || this.paying()) {
      return;
    }
    const entry = this.currentUserEntry(detail);
    if (!entry) {
      this.toastr.error(this.translation.translate('myPickups.details.payShareError'));
      return;
    }

    this.paying.set(true);
    this.orderService.payForOrder(detail.garbageGroupId, detail.id)
      .pipe(
        takeUntilDestroyed(),
        finalize(() => this.paying.set(false))
      )
      .subscribe((res) => {
        const updated = res.resultModel ?? null;
        if (updated) {
          this.order.set(updated);
        }
        this.toastr.success(this.translation.translate('myPickups.details.paySuccess'));
      });
  }

  private resolveOrder(orderId: string): void {
    const cached = this.orderService.findOrderById(orderId);
    if (cached) {
      this.order.set(cached);
      this.error.set(null);
      this.loading.set(false);
      return;
    }

    this.loading.set(true);
    this.error.set(null);
    this.order.set(null);

    this.orderService.getMyOrders(1, USER_ORDERS_PAGE_SIZE)
      .pipe(
        takeUntilDestroyed(),
        finalize(() => this.loading.set(false))
      )
      .subscribe((res) => {
        if (res.errorMessage) {
          return;
        }
        const refreshed = this.orderService.findOrderById(orderId);
        if (refreshed) {
          this.order.set(refreshed);
          return;
        }
        const message = this.translation.translate('myPickups.details.notFound');
        this.error.set(message);
      });
  }

  private currentUserEntry(pickup: GarbageOrderDto): { hasAcceptedPayment: boolean; shareAmount: number } | null {
    const me = this.currentUser();
    if (!me) {
      return null;
    }
    const entry = pickup.users.find((user) => user.userId === me.id);
    return entry ? { hasAcceptedPayment: entry.hasAcceptedPayment, shareAmount: entry.shareAmount } : null;
  }
}
