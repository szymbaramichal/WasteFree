import { GarbageAdminOrderDto, GarbageOrderStatus, PickupOption, ContainerSize } from '@app/_models/garbage-orders';

export interface GarbageAdminOrderItem {
  id: string;
  raw: GarbageAdminOrderDto;
  orderNumber: string;
  schedule: string | null;
  scheduleType: 'pickup' | 'dropOff' | 'none';
  dropOffSchedule: string | null;
  pickupSchedule: string | null;
  isContainer: boolean;
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
  addressLine: string | null;
}

export interface PageMeta {
  pageSize: number;
  totalCount: number;
  totalPages: number;
  currentPage: number;
  start: number;
  end: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

export const PAGE_SIZE = 10;

export const STATUS_TRANSLATION_KEYS: Record<number, string> = {
  [GarbageOrderStatus.WaitingForPayment]: 'garbageAdminOrders.status.waitingForPayment',
  [GarbageOrderStatus.WaitingForAccept]: 'garbageAdminOrders.status.waitingForAccept',
  [GarbageOrderStatus.WaitingForPickup]: 'garbageAdminOrders.status.waitingForPickup',
  [GarbageOrderStatus.WaitingForUtilizationFee]: 'garbageAdminOrders.status.waitingForUtilizationFee',
  [GarbageOrderStatus.Completed]: 'garbageAdminOrders.status.completed',
  [GarbageOrderStatus.Complained]: 'garbageAdminOrders.status.complained',
  [GarbageOrderStatus.Resolved]: 'garbageAdminOrders.status.resolved',
  [GarbageOrderStatus.Cancelled]: 'garbageAdminOrders.status.cancelled'
};

export const STATUS_CLASS_MAP: Record<number, string> = {
  [GarbageOrderStatus.WaitingForPayment]: 'status-pill status-pill--pending',
  [GarbageOrderStatus.WaitingForAccept]: 'status-pill status-pill--pending',
  [GarbageOrderStatus.WaitingForPickup]: 'status-pill status-pill--active',
  [GarbageOrderStatus.WaitingForUtilizationFee]: 'status-pill status-pill--active',
  [GarbageOrderStatus.Completed]: 'status-pill status-pill--completed',
  [GarbageOrderStatus.Complained]: 'status-pill status-pill--flagged',
  [GarbageOrderStatus.Resolved]: 'status-pill status-pill--resolved',
  [GarbageOrderStatus.Cancelled]: 'status-pill status-pill--cancelled'
};

export const PICKUP_OPTION_KEYS: Record<number, string> = {
  [PickupOption.SmallPickup]: 'garbageAdminOrders.pickupOption.smallPickup',
  [PickupOption.Pickup]: 'garbageAdminOrders.pickupOption.pickup',
  [PickupOption.Container]: 'garbageAdminOrders.pickupOption.container',
  [PickupOption.SpecialOrder]: 'garbageAdminOrders.pickupOption.specialOrder'
};

export const CONTAINER_SIZE_KEYS: Record<number, string> = {
  [ContainerSize.ContainerSmall]: 'garbageAdminOrders.containerSize.small',
  [ContainerSize.ContainerMedium]: 'garbageAdminOrders.containerSize.medium',
  [ContainerSize.ContainerLarge]: 'garbageAdminOrders.containerSize.large'
};
