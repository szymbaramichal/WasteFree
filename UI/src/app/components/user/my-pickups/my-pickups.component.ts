import { CommonModule } from '@angular/common';
import { Component, computed, effect, inject, signal } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { TranslatePipe } from '@app/pipes/translate.pipe';
import { GarbageOrderDto, GarbageOrderStatus, PickupOption } from '@app/_models/garbage-orders';
import { GarbageOrderService, USER_ORDERS_PAGE_SIZE } from '@app/services/garbage-order.service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs/operators';

type StatusFilter = GarbageOrderStatus | 'all';
type GroupFilter = string | 'all';

const STATUS_OPTIONS: GarbageOrderStatus[] = [
  GarbageOrderStatus.WaitingForPayment,
  GarbageOrderStatus.WaitingForAccept,
  GarbageOrderStatus.WaitingForPickup,
  GarbageOrderStatus.WaitingForUtilizationFee,
  GarbageOrderStatus.Completed,
  GarbageOrderStatus.Complained,
  GarbageOrderStatus.Resolved,
  GarbageOrderStatus.Cancelled
];

@Component({
  selector: 'app-my-pickups',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslatePipe],
  templateUrl: './my-pickups.component.html',
  styleUrls: ['./my-pickups.component.css']
})
export class MyPickupsComponent {
  private orderService = inject(GarbageOrderService);
  private router = inject(Router);

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly pickups = this.orderService.orders;
  readonly itemsPerPage = 20;
  readonly statusOptions: GarbageOrderStatus[] = STATUS_OPTIONS;
  readonly statusFilter = signal<StatusFilter>('all');
  readonly groupFilter = signal<GroupFilter>('all');
  readonly currentPage = signal(1);
  readonly filteredPickups = computed(() => {
    const statusFilter = this.statusFilter();
    const groupFilter = this.groupFilter();
    return this.pickups().filter((pickup) => {
      const matchesStatus = statusFilter === 'all' || pickup.garbageOrderStatus === statusFilter;
      const matchesGroup = groupFilter === 'all' || pickup.garbageGroupId === groupFilter;
      return matchesStatus && matchesGroup;
    });
  });
  readonly groupOptions = computed(() => {
    const groups = new Map<string, string>();
    for (const pickup of this.pickups()) {
      if (!groups.has(pickup.garbageGroupId)) {
        groups.set(pickup.garbageGroupId, pickup.garbageGroupName);
      }
    }
    return Array.from(groups.entries()).map(([id, name]) => ({ id, name }));
  });
  readonly totalPages = computed(() => Math.ceil(this.filteredPickups().length / this.itemsPerPage));
  readonly paginatedPickups = computed(() => {
    const start = (this.currentPage() - 1) * this.itemsPerPage;
    return this.filteredPickups().slice(start, start + this.itemsPerPage);
  });
  readonly pages = computed(() => {
    const total = this.totalPages();
    return Array.from({ length: total }, (_value, index) => index + 1);
  });
  readonly pageStartIndex = computed(() => {
    if (!this.filteredPickups().length) {
      return 0;
    }
    return (this.currentPage() - 1) * this.itemsPerPage;
  });
  readonly pageEndIndex = computed(() => {
    if (!this.filteredPickups().length) {
      return 0;
    }
    return Math.min(this.pageStartIndex() + this.itemsPerPage, this.filteredPickups().length);
  });

  constructor() {
    this.loadOrders();

    effect(() => {
      const total = this.totalPages();
      const current = this.currentPage();
      if (total === 0) {
        if (current !== 1) {
          this.currentPage.set(1);
        }
        return;
      }

      if (current > total) {
        this.currentPage.set(total);
      }
    });
  }

  private loadOrders(): void {
    this.loading.set(true);
    this.error.set(null);
    this.orderService.ensureMyOrders(1, USER_ORDERS_PAGE_SIZE)
      .pipe(
        takeUntilDestroyed(),
        finalize(() => this.loading.set(false))
      )
      .subscribe((res) => {
        if (res.errorMessage) {
          return;
        }
      });
  }

  trackByOrder(_index: number, pickup: GarbageOrderDto): string {
    return pickup.id;
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

  setStatusFilter(raw: StatusFilter | string): void {
    const value: StatusFilter = raw === 'all' ? 'all' : Number(raw) as GarbageOrderStatus;
    this.statusFilter.set(value);
    this.currentPage.set(1);
  }

  setGroupFilter(raw: GroupFilter | string): void {
    const value: GroupFilter = raw === 'all' ? 'all' : String(raw);
    this.groupFilter.set(value as GroupFilter);
    this.currentPage.set(1);
  }

  goToPage(page: number): void {
    const total = this.totalPages();
    if (total === 0) {
      this.currentPage.set(1);
      return;
    }

    const clamped = Math.min(Math.max(page, 1), total);
    this.currentPage.set(clamped);
  }

  previousPage(): void {
    if (this.currentPage() > 1) {
      this.currentPage.update((current) => current - 1);
    }
  }

  nextPage(): void {
    const total = this.totalPages();
    if (total > 0 && this.currentPage() < total) {
      this.currentPage.update((current) => current + 1);
    }
  }

  openDetails(pickup: GarbageOrderDto): void {
    this.router.navigate(['/portal/my-pickups', pickup.id]);
  }

  orderCode(pickup: GarbageOrderDto): string {
    return pickup.id?.slice(0, 8)?.toUpperCase() ?? pickup.id;
  }
}
