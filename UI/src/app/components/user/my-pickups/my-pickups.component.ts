import { CommonModule } from '@angular/common';
import {
  Component,
  DestroyRef,
  OnInit,
  computed,
  effect,
  inject,
  signal
} from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { TranslatePipe } from '@app/pipes/translate.pipe';
import { forkJoin, of } from 'rxjs';
import { finalize, map, switchMap } from 'rxjs/operators';
import { Pager, PaginatedResult } from '@app/_models/result';
import { MyPickupDto } from '@app/_models/pickups';
import { MyPickupsService } from '@app/services/my-pickups.service';
import { PickupOptionKey } from '@app/_models/profile';

interface PortalPickupItem {
  id: string;
  orderNumber: string;
  groupId: string | null;
  groupName: string;
  status: PickupStatusKey;
  cost: number;
  pickupDate: string | null;
  pickupOption: PickupOptionKey;
  isHighPriority: boolean;
}

type PickupStatusKey =
  | 'waitingForPayment'
  | 'waitingForAccept'
  | 'waitingForPickup'
  | 'waitingForUtilizationFee'
  | 'completed'
  | 'complained'
  | 'resolved'
  | 'cancelled';

type StatusFilter = PickupStatusKey | 'all';
type GroupFilter = string | 'all';

const STATUS_VALUE_TO_KEY: Record<number, PickupStatusKey> = {
  0: 'waitingForPayment',
  1: 'waitingForAccept',
  2: 'waitingForPickup',
  3: 'waitingForUtilizationFee',
  4: 'completed',
  5: 'complained',
  6: 'resolved',
  7: 'cancelled'
};

const PICKUP_OPTION_VALUE_TO_KEY: Record<number, PickupOptionKey> = {
  0: 'smallPickup',
  1: 'pickup',
  2: 'container',
  3: 'specialOrder'
};

const STATUS_TRANSLATION_KEY: Record<PickupStatusKey, string> = {
  waitingForPayment: 'myPickups.status.waitingForPayment',
  waitingForAccept: 'myPickups.status.waitingForAccept',
  waitingForPickup: 'myPickups.status.waitingForPickup',
  waitingForUtilizationFee: 'myPickups.status.waitingForUtilizationFee',
  completed: 'myPickups.status.completed',
  complained: 'myPickups.status.complained',
  resolved: 'myPickups.status.resolved',
  cancelled: 'myPickups.status.cancelled'
};

const STATUS_ORDER: PickupStatusKey[] = [
  'waitingForPayment',
  'waitingForAccept',
  'waitingForPickup',
  'waitingForUtilizationFee',
  'completed',
  'complained',
  'resolved',
  'cancelled'
];

const PICKUP_OPTION_TRANSLATION_KEY: Record<PickupOptionKey, string> = {
  smallPickup: 'myPickups.option.smallPickup',
  pickup: 'myPickups.option.pickup',
  container: 'myPickups.option.container',
  specialOrder: 'myPickups.option.specialOrder'
};

const UNKNOWN_GROUP_NAME = 'N/A';
const DEFAULT_ITEMS_PER_PAGE = 10;

@Component({
  selector: 'app-my-pickups',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslatePipe],
  templateUrl: './my-pickups.component.html',
  styleUrls: ['./my-pickups.component.css']
})
export class MyPickupsComponent implements OnInit {
  private readonly myPickupsService = inject(MyPickupsService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly router = inject(Router);

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly pickups = signal<PortalPickupItem[]>([]);
  readonly totalCount = signal(0);
  readonly itemsPerPage = DEFAULT_ITEMS_PER_PAGE;
  readonly apiPageSize = this.itemsPerPage;
  readonly statusOptions = signal<PickupStatusKey[]>([]);
  readonly statusFilter = signal<StatusFilter>('all');
  readonly groupFilter = signal<GroupFilter>('all');
  readonly currentPage = signal(1);

  readonly filteredPickups = computed(() => {
    const statusFilter = this.statusFilter();
    const groupFilter = this.groupFilter();

    return this.pickups().filter((pickup) => {
      const matchesStatus = statusFilter === 'all' || pickup.status === statusFilter;
      const matchesGroup = groupFilter === 'all' || pickup.groupId === groupFilter;
      return matchesStatus && matchesGroup;
    });
  });

  readonly groupOptions = computed(() => {
    const groups = new Map<string, string>();

    for (const pickup of this.pickups()) {
      if (pickup.groupId && !groups.has(pickup.groupId)) {
        groups.set(pickup.groupId, pickup.groupName);
      }
    }

    return Array.from(groups.entries()).map(([id, name]) => ({ id, name }));
  });

  readonly totalPages = computed(() => {
    const totalItems = this.filteredPickups().length;
    return totalItems === 0 ? 0 : Math.ceil(totalItems / this.itemsPerPage);
  });

  readonly paginatedPickups = computed(() => {
    const start = (this.currentPage() - 1) * this.itemsPerPage;
    return this.filteredPickups().slice(start, start + this.itemsPerPage);
  });

  readonly pages = computed(() => {
    const total = this.totalPages();
    return total === 0 ? [] : Array.from({ length: total }, (_value, index) => index + 1);
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

  ngOnInit(): void {
    this.fetchPickups();
  }

  trackByOrder(_index: number, pickup: PortalPickupItem): string {
    return pickup.id;
  }

  setStatusFilter(raw: StatusFilter | string): void {
    const value: StatusFilter = raw === 'all' ? 'all' : (raw as PickupStatusKey);
    this.statusFilter.set(value);
    this.currentPage.set(1);
  }

  setGroupFilter(raw: GroupFilter | string): void {
    const value: GroupFilter = raw === 'all' ? 'all' : String(raw);
    this.groupFilter.set(value);
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

  openDetails(pickup: PortalPickupItem): void {
    this.router.navigate(['/portal/my-pickups', pickup.id]);
  }

  orderCode(pickup: PortalPickupItem): string {
    return pickup.orderNumber;
  }

  statusClass(status: PickupStatusKey): string {
    switch (status) {
      case 'completed':
        return 'status-chip status-completed';
      case 'waitingForPickup':
      case 'waitingForAccept':
      case 'waitingForPayment':
      case 'waitingForUtilizationFee':
      case 'resolved':
        return 'status-chip status-scheduled';
      case 'complained':
        return 'status-chip status-complained';
      case 'cancelled':
        return 'status-chip status-cancelled';
      default:
        return 'status-chip status-pending';
    }
  }

  statusTranslationKey(status: PickupStatusKey): string {
    return STATUS_TRANSLATION_KEY[status];
  }

  pickupOptionKey(option: PickupOptionKey): string {
    return PICKUP_OPTION_TRANSLATION_KEY[option] ?? PICKUP_OPTION_TRANSLATION_KEY.pickup;
  }

  private fetchPickups(): void {
    this.loading.set(true);
    this.error.set(null);

    this.myPickupsService
      .getMyPickups(1, this.apiPageSize)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
  switchMap((firstPage: PaginatedResult<MyPickupDto[]>) => {
          const pager = firstPage?.pager;
          const initialItems: MyPickupDto[] = Array.isArray(firstPage?.resultModel)
            ? firstPage.resultModel
            : [];
          const totalPages = pager?.totalPages ?? 1;

          if (totalPages <= 1) {
            return of({ items: initialItems, pager });
          }

          const requests: ReturnType<MyPickupsService['getMyPickups']>[] = [];

          for (let page = 2; page <= totalPages; page++) {
            requests.push(this.myPickupsService.getMyPickups(page, this.apiPageSize));
          }

          if (!requests.length) {
            return of({ items: initialItems, pager });
          }

          return forkJoin(requests).pipe(
            map((responses: PaginatedResult<MyPickupDto[]>[]) => {
              const combined = responses.reduce<MyPickupDto[]>((acc, res) => {
                if (Array.isArray(res?.resultModel)) {
                  acc.push(...res.resultModel);
                }
                return acc;
              }, [...initialItems]);

              let resolvedPager: Pager | null = pager ?? null;

              if (responses.length) {
                const tailPager = responses[responses.length - 1]?.pager ?? null;
                if (tailPager) {
                  resolvedPager = tailPager;
                }
              }

              return { items: combined, pager: resolvedPager };
            })
          );
        }),
        finalize(() => this.loading.set(false))
      )
      .subscribe({
        next: (payload: { items: MyPickupDto[]; pager: Pager | null }) => {
          const mapped = payload.items.map((item) => this.toPortalPickup(item));
          this.pickups.set(mapped);
          this.totalCount.set(payload.pager?.totalCount ?? mapped.length);
          this.currentPage.set(1);

          const uniqueStatuses = STATUS_ORDER.filter((status) =>
            mapped.some((item) => item.status === status)
          );

          this.statusOptions.set(uniqueStatuses);

          const currentStatus = this.statusFilter();
          if (currentStatus !== 'all' && !uniqueStatuses.includes(currentStatus)) {
            this.statusFilter.set('all');
          }
        },
        error: () => {
          this.error.set('myPickups.loadError');
        }
      });
  }

  private toPortalPickup(dto: MyPickupDto): PortalPickupItem {
    const status = STATUS_VALUE_TO_KEY[dto.garbageOrderStatus] ?? 'waitingForPayment';
    const pickupOption = PICKUP_OPTION_VALUE_TO_KEY[dto.pickupOption] ?? 'pickup';
    const groupName = dto.garbageGroupName?.trim() || UNKNOWN_GROUP_NAME;
    const groupId = dto.garbageGroupId?.trim() || null;
    const pickupDate = dto.pickupDate ?? dto.dropOffDate;
    const cost = typeof dto.cost === 'number' && Number.isFinite(dto.cost) ? dto.cost : 0;

    return {
      id: dto.id,
      orderNumber: this.formatOrderNumber(dto.id),
      groupId,
      groupName,
      status,
      cost,
      pickupDate,
      pickupOption,
      isHighPriority: dto.isHighPriority
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
