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
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { TranslatePipe } from '@app/pipes/translate.pipe';
import {
  GarbageAdminOrderDto,
  PickupOption
} from '@app/_models/garbage-orders';
import { Address, formatLocalizedAddress } from '@app/_models/address';
import { Pager, PaginatedResult } from '@app/_models/result';
import { GarbageAdminOrdersService } from '@app/services/garbage-admin-orders.service';
import { TranslationService } from '@app/services/translation.service';
import { finalize } from 'rxjs/operators';
import {
  GarbageAdminOrderItem,
  PageMeta,
  PAGE_SIZE,
  STATUS_TRANSLATION_KEYS,
  STATUS_CLASS_MAP,
  PICKUP_OPTION_KEYS,
  CONTAINER_SIZE_KEYS
} from '../garbage-admin-orders.types';
import { UtilizationFeeModalComponent } from '../utilization-fee-modal/utilization-fee-modal.component';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-garbage-admin-orders-assigned',
  standalone: true,
  imports: [CommonModule, TranslatePipe, UtilizationFeeModalComponent],
  templateUrl: './garbage-admin-orders-assigned.component.html',
  styleUrls: ['./garbage-admin-orders-assigned.component.css']
})
export class GarbageAdminOrdersAssignedComponent implements OnInit {
  private readonly ordersService = inject(GarbageAdminOrdersService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly translation = inject(TranslationService);
  private readonly toastr = inject(ToastrService);

  readonly currentLang = toSignal(this.translation.onLangChange, { initialValue: this.translation.currentLang });

  private readonly pageSize = PAGE_SIZE;

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly pager = signal<Pager | null>(null);
  readonly page = signal(1);
  readonly items = signal<GarbageAdminOrderItem[]>([]);

  readonly feeModalVisible = signal(false);
  readonly feeModalLoading = signal(false);
  readonly feeModalError = signal<string | null>(null);
  readonly feeModalOrderId = signal<string | null>(null);

  readonly pageMeta = computed(() =>
    this.resolvePageMeta(this.pager(), this.items().length, this.page())
  );

  private readonly localizedAddressesEffect = effect(() => {
    const lang = this.currentLang();
    const items = this.items();

    if (!items.length) {
      return;
    }

    let changed = false;
    const nextItems = items.map((item) => {
      const nextAddress = formatLocalizedAddress(item.raw?.garbageGroupAddress, lang);
      if (nextAddress === item.addressLine) {
        return item;
      }
      changed = true;
      return { ...item, addressLine: nextAddress };
    });

    if (changed) {
      this.items.set(nextItems);
    }
  });

  ngOnInit(): void {
    this.load();
  }

  trackById(_index: number, item: GarbageAdminOrderItem): string {
    return item.id;
  }

  openChat(orderId: string): void {
    // Mock modal for chat
    alert('Chat functionality coming soon for order: ' + orderId);
  }

  openFeeModal(orderId: string): void {
    this.feeModalOrderId.set(orderId);
    this.feeModalVisible.set(true);
    this.feeModalError.set(null);
  }

  closeFeeModal(): void {
    this.feeModalVisible.set(false);
    this.feeModalOrderId.set(null);
    this.feeModalError.set(null);
  }

  submitFee(data: { amount: number; proof: File }): void {
    const orderId = this.feeModalOrderId();
    if (!orderId) return;

    this.feeModalLoading.set(true);
    this.feeModalError.set(null);

    this.ordersService.submitUtilizationFee(orderId, data.amount, data.proof)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.feeModalLoading.set(false))
      )
      .subscribe({
        next: () => {
          this.toastr.success(this.translation.translate('garbageAdminOrders.utilizationFee.submitSuccess'));
          this.closeFeeModal();
          this.refresh();
        },
        error: (err) => {
          const errorCode = err?.error?.errorCode;
          if (errorCode === 'UNSUPPORTED_IMAGE_TYPE') {
            this.feeModalError.set('garbageAdminOrders.utilizationFee.errors.unsupportedType');
          } else if (errorCode === 'INVALID_ORDER_STATUS') {
            this.feeModalError.set('garbageAdminOrders.utilizationFee.errors.invalidStatus');
          } else {
            this.feeModalError.set('garbageAdminOrders.utilizationFee.submitError');
          }
        }
      });
  }

  refresh(): void {
    this.load(this.page());
  }

  retry(): void {
    this.load(this.page());
  }

  goToPage(page: number): void {
    this.load(page);
  }

  previousPage(): void {
    const page = Math.max(1, this.page() - 1);
    this.goToPage(page);
  }

  nextPage(): void {
    const meta = this.pageMeta();
    if (meta.hasNext) {
      this.goToPage(this.page() + 1);
    }
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

  private load(page = this.page()): void {
    const requested = Math.max(1, page);
    this.loading.set(true);
    this.error.set(null);

    this.ordersService
      .getCurrentOrders(requested, this.pageSize)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false))
      )
      .subscribe({
        next: (res) => this.handleResponse(res, requested),
        error: () => this.error.set('garbageAdminOrders.current.loadError')
      });
  }

  private handleResponse(res: PaginatedResult<GarbageAdminOrderDto[]>, requestedPage: number): void {
    const dtos = Array.isArray(res.resultModel) ? res.resultModel : [];
    const items = dtos.map((dto) => this.toItem(dto));
    const pager = res.pager ?? null;

    this.pager.set(pager);

    const totalPages = pager?.totalPages ?? (items.length ? 1 : 0);

    if (totalPages > 0 && requestedPage > totalPages) {
      this.page.set(totalPages);
      this.load(totalPages);
      return;
    }

    if (totalPages === 0 && requestedPage > 1) {
      this.page.set(1);
      this.load(1);
      return;
    }

    this.items.set(items);
    const resolvedPage = pager?.pageNumber ?? (totalPages === 0 ? 1 : requestedPage);
    this.page.set(resolvedPage);
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
    const dropOffSchedule = dto.dropOffDate ?? null;
    const pickupSchedule = dto.pickupDate ?? null;
    const isContainer = dto.pickupOption === PickupOption.Container;
    const schedule = pickupSchedule ?? dropOffSchedule;
    const scheduleType: GarbageAdminOrderItem['scheduleType'] = pickupSchedule
      ? 'pickup'
      : dropOffSchedule
      ? 'dropOff'
      : 'none';

    return {
      id: dto.id,
      raw: dto,
      orderNumber: this.formatOrderNumber(dto.id),
      schedule,
      scheduleType,
      dropOffSchedule,
      pickupSchedule,
      isContainer,
      pickupOptionKey,
      containerSizeKey,
      statusKey,
      statusClass,
      isHighPriority: dto.isHighPriority,
      collectingService: dto.collectingService,
      groupName: dto.garbageGroupName?.trim() || null,
      isPrivateGroup: dto.garbageGroupIsPrivate,
      cost: dto.cost,
      distance: dto.distanceInKilometers,
      addressLine: this.formatAddress(dto.garbageGroupAddress)
    };
  }

  private formatOrderNumber(id: string): string {
    const cleaned = id?.replace(/[^a-zA-Z0-9]/g, '').toUpperCase();

    if (!cleaned) {
      return 'N/A';
    }

    return cleaned.length <= 8 ? cleaned : `${cleaned.slice(0, 4)}-${cleaned.slice(-4)}`;
  }

  private formatAddress(address: Address | null | undefined): string | null {
    return formatLocalizedAddress(address, this.currentLang());
  }
}
