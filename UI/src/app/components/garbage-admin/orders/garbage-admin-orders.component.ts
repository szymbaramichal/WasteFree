import { CommonModule } from '@angular/common';
import {
  Component,
  DestroyRef,
  OnInit,
  computed,
  inject,
  signal
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { TranslatePipe } from '@app/pipes/translate.pipe';
import {
  ContainerSize,
  GarbageAdminOrderDto,
  GarbageOrderStatus,
  PickupOption
} from '@app/_models/garbage-orders';
import { Pager, PaginatedResult } from '@app/_models/result';
import { GarbageAdminOrdersService } from '@app/services/garbage-admin-orders.service';
import { finalize } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';
import { TranslationService } from '@app/services/translation.service';

interface GarbageAdminOrderItem {
  id: string;
  raw: GarbageAdminOrderDto;
  orderNumber: string;
  schedule: string | null;
  scheduleType: 'pickup' | 'dropOff' | 'none';
  pickupOptionKey: string;
  containerSizeKey: string | null;
  statusKey: string;
  statusClass: string;
  isHighPriority: boolean;
  collectingService: boolean;
  groupName: string | null;
  isPrivateGroup: boolean;
  cost: number;
  distance: number | null;
}

interface PageMeta {
  pageSize: number;
  totalCount: number;
  totalPages: number;
  currentPage: number;
  start: number;
  end: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

const PAGE_SIZE = 10;

const STATUS_TRANSLATION_KEYS: Record<number, string> = {
  [GarbageOrderStatus.WaitingForPayment]: 'garbageAdminOrders.status.waitingForPayment',
  [GarbageOrderStatus.WaitingForAccept]: 'garbageAdminOrders.status.waitingForAccept',
  [GarbageOrderStatus.WaitingForPickup]: 'garbageAdminOrders.status.waitingForPickup',
  [GarbageOrderStatus.WaitingForUtilizationFee]: 'garbageAdminOrders.status.waitingForUtilizationFee',
  [GarbageOrderStatus.Completed]: 'garbageAdminOrders.status.completed',
  [GarbageOrderStatus.Complained]: 'garbageAdminOrders.status.complained',
  [GarbageOrderStatus.Resolved]: 'garbageAdminOrders.status.resolved',
  [GarbageOrderStatus.Cancelled]: 'garbageAdminOrders.status.cancelled'
};

const STATUS_CLASS_MAP: Record<number, string> = {
  [GarbageOrderStatus.WaitingForPayment]: 'status-pill status-pill--pending',
  [GarbageOrderStatus.WaitingForAccept]: 'status-pill status-pill--pending',
  [GarbageOrderStatus.WaitingForPickup]: 'status-pill status-pill--active',
  [GarbageOrderStatus.WaitingForUtilizationFee]: 'status-pill status-pill--active',
  [GarbageOrderStatus.Completed]: 'status-pill status-pill--completed',
  [GarbageOrderStatus.Complained]: 'status-pill status-pill--flagged',
  [GarbageOrderStatus.Resolved]: 'status-pill status-pill--resolved',
  [GarbageOrderStatus.Cancelled]: 'status-pill status-pill--cancelled'
};

const PICKUP_OPTION_KEYS: Record<number, string> = {
  [PickupOption.SmallPickup]: 'garbageAdminOrders.pickupOption.smallPickup',
  [PickupOption.Pickup]: 'garbageAdminOrders.pickupOption.pickup',
  [PickupOption.Container]: 'garbageAdminOrders.pickupOption.container',
  [PickupOption.SpecialOrder]: 'garbageAdminOrders.pickupOption.specialOrder'
};

const CONTAINER_SIZE_KEYS: Record<number, string> = {
  [ContainerSize.ContainerSmall]: 'garbageAdminOrders.containerSize.small',
  [ContainerSize.ContainerMedium]: 'garbageAdminOrders.containerSize.medium',
  [ContainerSize.ContainerLarge]: 'garbageAdminOrders.containerSize.large'
};

@Component({
  selector: 'app-garbage-admin-orders',
  standalone: true,
  imports: [CommonModule, TranslatePipe],
  templateUrl: './garbage-admin-orders.component.html',
  styleUrls: ['./garbage-admin-orders.component.css']
})
export class GarbageAdminOrdersComponent implements OnInit {
  private readonly ordersService = inject(GarbageAdminOrdersService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly toastr = inject(ToastrService);
  private readonly translation = inject(TranslationService);

  private readonly pageSize = PAGE_SIZE;

  readonly waitingLoading = signal(false);
  readonly waitingError = signal<string | null>(null);
  readonly waitingPager = signal<Pager | null>(null);
  readonly waitingPage = signal(1);
  readonly waitingItems = signal<GarbageAdminOrderItem[]>([]);

  readonly currentLoading = signal(false);
  readonly currentError = signal<string | null>(null);
  readonly currentPager = signal<Pager | null>(null);
  readonly currentPage = signal(1);
  readonly currentItems = signal<GarbageAdminOrderItem[]>([]);

  readonly acceptingIds = signal<Set<string>>(new Set());

  readonly waitingPageMeta = computed(() =>
    this.resolvePageMeta(this.waitingPager(), this.waitingItems().length, this.waitingPage())
  );

  readonly currentPageMeta = computed(() =>
    this.resolvePageMeta(this.currentPager(), this.currentItems().length, this.currentPage())
  );

  ngOnInit(): void {
    this.loadWaiting();
    this.loadCurrent();
  }

  trackById(_index: number, item: GarbageAdminOrderItem): string {
    return item.id;
  }

  isAccepting(id: string): boolean {
    return this.acceptingIds().has(id);
  }

  refreshAll(): void {
    this.loadWaiting(this.waitingPage());
    this.loadCurrent(this.currentPage());
  }

  retryWaiting(): void {
    this.loadWaiting(this.waitingPage());
  }

  retryCurrent(): void {
    this.loadCurrent(this.currentPage());
  }

  goToWaitingPage(page: number): void {
    this.loadWaiting(page);
  }

  previousWaitingPage(): void {
    const page = Math.max(1, this.waitingPage() - 1);
    this.goToWaitingPage(page);
  }

  nextWaitingPage(): void {
    const meta = this.waitingPageMeta();
    if (meta.hasNext) {
      this.goToWaitingPage(this.waitingPage() + 1);
    }
  }

  goToCurrentPage(page: number): void {
    this.loadCurrent(page);
  }

  previousCurrentPage(): void {
    const page = Math.max(1, this.currentPage() - 1);
    this.goToCurrentPage(page);
  }

  nextCurrentPage(): void {
    const meta = this.currentPageMeta();
    if (meta.hasNext) {
      this.goToCurrentPage(this.currentPage() + 1);
    }
  }

  acceptOrder(item: GarbageAdminOrderItem): void {
    if (this.isAccepting(item.id)) {
      return;
    }

    this.acceptingIds.update((current) => {
      const next = new Set(current);
      next.add(item.id);
      return next;
    });

    this.ordersService
      .acceptOrder(item.id)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.acceptingIds.update((current) => {
            const next = new Set(current);
            next.delete(item.id);
            return next;
          });
        })
      )
      .subscribe({
        next: () => {
          this.toastr.success(this.translation.translate('garbageAdminOrders.accept.success'));
          this.loadWaiting(this.waitingPage());
          this.loadCurrent(this.currentPage());
        },
        error: () => {
          this.toastr.error(this.translation.translate('garbageAdminOrders.accept.error'));
        }
      });
  }

  scheduleKey(item: GarbageAdminOrderItem): string {
    switch (item.scheduleType) {
      case 'pickup':
        return 'garbageAdminOrders.schedule.pickup';
      case 'dropOff':
        return 'garbageAdminOrders.schedule.dropOff';
      default:
        return 'garbageAdminOrders.schedule.none';
    }
  }

  private loadWaiting(page = this.waitingPage()): void {
    const requested = Math.max(1, page);
    this.waitingLoading.set(true);
    this.waitingError.set(null);

    this.ordersService
      .getWaitingOrders(requested, this.pageSize)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.waitingLoading.set(false))
      )
      .subscribe({
        next: (res) => this.handleWaitingResponse(res, requested),
        error: () => this.waitingError.set('garbageAdminOrders.waiting.loadError')
      });
  }

  private loadCurrent(page = this.currentPage()): void {
    const requested = Math.max(1, page);
    this.currentLoading.set(true);
    this.currentError.set(null);

    this.ordersService
      .getCurrentOrders(requested, this.pageSize)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.currentLoading.set(false))
      )
      .subscribe({
        next: (res) => this.handleCurrentResponse(res, requested),
        error: () => this.currentError.set('garbageAdminOrders.current.loadError')
      });
  }

  private handleWaitingResponse(res: PaginatedResult<GarbageAdminOrderDto[]>, requestedPage: number): void {
    const dtos = Array.isArray(res.resultModel) ? res.resultModel : [];
    const items = dtos.map((dto) => this.toItem(dto));
    const pager = res.pager ?? null;

    this.waitingPager.set(pager);

    const totalPages = pager?.totalPages ?? (items.length ? 1 : 0);

    if (totalPages > 0 && requestedPage > totalPages) {
      this.waitingPage.set(totalPages);
      this.loadWaiting(totalPages);
      return;
    }

    if (totalPages === 0 && requestedPage > 1) {
      this.waitingPage.set(1);
      this.loadWaiting(1);
      return;
    }

    this.waitingItems.set(items);
    const resolvedPage = pager?.pageNumber ?? (totalPages === 0 ? 1 : requestedPage);
    this.waitingPage.set(resolvedPage);
  }

  private handleCurrentResponse(res: PaginatedResult<GarbageAdminOrderDto[]>, requestedPage: number): void {
    const dtos = Array.isArray(res.resultModel) ? res.resultModel : [];
    const items = dtos.map((dto) => this.toItem(dto));
    const pager = res.pager ?? null;

    this.currentPager.set(pager);

    const totalPages = pager?.totalPages ?? (items.length ? 1 : 0);

    if (totalPages > 0 && requestedPage > totalPages) {
      this.currentPage.set(totalPages);
      this.loadCurrent(totalPages);
      return;
    }

    if (totalPages === 0 && requestedPage > 1) {
      this.currentPage.set(1);
      this.loadCurrent(1);
      return;
    }

    this.currentItems.set(items);
    const resolvedPage = pager?.pageNumber ?? (totalPages === 0 ? 1 : requestedPage);
    this.currentPage.set(resolvedPage);
  }

  private resolvePageMeta(pager: Pager | null, itemsLength: number, currentPage: number): PageMeta {
    const pageSize = pager?.pageSize ?? this.pageSize;
    const totalCount = pager?.totalCount ?? itemsLength;
    const totalPages = pager?.totalPages ?? (totalCount === 0 ? 0 : Math.ceil(totalCount / pageSize));
    const normalizedPage = totalPages === 0 ? 1 : Math.min(Math.max(currentPage, 1), totalPages);
    const start = totalCount === 0 ? 0 : (normalizedPage - 1) * pageSize + 1;
    const end = totalCount === 0 ? 0 : Math.min(start + pageSize - 1, totalCount);

    return {
      pageSize,
      totalCount,
      totalPages,
      currentPage: totalPages === 0 ? 0 : normalizedPage,
      start,
      end,
      hasPrevious: totalPages > 0 && normalizedPage > 1,
      hasNext: totalPages > 0 && normalizedPage < totalPages
    };
  }

  private toItem(dto: GarbageAdminOrderDto): GarbageAdminOrderItem {
    const statusKey = STATUS_TRANSLATION_KEYS[dto.garbageOrderStatus] ?? 'garbageAdminOrders.status.unknown';
    const statusClass = STATUS_CLASS_MAP[dto.garbageOrderStatus] ?? 'status-pill';
    const pickupOptionKey = PICKUP_OPTION_KEYS[dto.pickupOption] ?? 'garbageAdminOrders.pickupOption.unknown';
    const containerSizeKey =
      dto.containerSize === null || dto.containerSize === undefined
        ? null
        : CONTAINER_SIZE_KEYS[dto.containerSize] ?? null;
    const schedule = dto.pickupDate ?? dto.dropOffDate ?? null;
    const scheduleType: GarbageAdminOrderItem['scheduleType'] = dto.pickupDate
      ? 'pickup'
      : dto.dropOffDate
      ? 'dropOff'
      : 'none';

    return {
      id: dto.id,
      raw: dto,
      orderNumber: this.formatOrderNumber(dto.id),
      schedule,
      scheduleType,
      pickupOptionKey,
      containerSizeKey,
      statusKey,
      statusClass,
      isHighPriority: dto.isHighPriority,
      collectingService: dto.collectingService,
      groupName: dto.garbageGroupName?.trim() || null,
      isPrivateGroup: dto.garbageGroupIsPrivate,
      cost: dto.cost,
      distance: dto.distanceInKilometers
    };
  }

  private formatOrderNumber(id: string): string {
    const cleaned = id?.replace(/[^a-zA-Z0-9]/g, '').toUpperCase();

    if (!cleaned) {
      return 'N/A';
    }

    return cleaned.length <= 8 ? cleaned : `${cleaned.slice(0, 4)}-${cleaned.slice(-4)}`;
  }
}
