export enum GarbageOrderStatus {
  WaitingForPayment = 0,
  WaitingForAccept = 1,
  WaitingForPickup = 2,
  WaitingForUtilizationFee = 3,
  Completed = 4,
  Complained = 5,
  Resolved = 6,
  Cancelled = 7
}

export enum PickupOption {
  SmallPickup = 0,
  Pickup = 1,
  Container = 2,
  SpecialOrder = 3
}

export enum ContainerSize {
  ContainerSmall = 0,
  ContainerMedium = 1,
  ContainerLarge = 2
}

export interface GarbageOrderFilterRequest {
  fromDate?: string | null;
  toDate?: string | null;
  pickupOption?: PickupOption | null;
  statuses?: GarbageOrderStatus[] | null;
}

export interface GarbageOrderCostDto {
  estimatedCost: number | null;
}

export interface CalculateGarbageOrderRequest {
  pickupOption: PickupOption;
  containerSize: ContainerSize | null;
  dropOffDate: string | null;
  pickupDate: string;
  isHighPriority: boolean;
  collectingService: boolean;
}

export interface GarbageOrderUserDto {
  userId: string;
  username: string;
  hasAcceptedPayment: boolean;
  shareAmount: number;
}

export interface GarbageOrderDto {
  id: string;
  pickupOption: PickupOption;
  containerSize: ContainerSize | null;
  dropOffDate: string | null;
  pickupDate: string;
  isHighPriority: boolean;
  collectingService: boolean;
  garbageOrderStatus: GarbageOrderStatus;
  cost: number;
  garbageGroupId: string;
  garbageGroupName: string;
  garbageGroupIsPrivate: boolean;
  users: GarbageOrderUserDto[];
}

export interface CreateGarbageOrderRequest {
  pickupOption: PickupOption;
  containerSize: ContainerSize | null;
  dropOffDate: string | null;
  pickupDate: string;
  isHighPriority: boolean;
  collectingService: boolean;
  userIds: string[];
}

export interface GarbageAdminOrderUserDto {
  userId: string;
  username: string | null;
  hasAcceptedPayment: boolean;
  shareAmount: number;
  additionalUtilizationFeeShareAmount: number | null;
}

export interface GarbageAdminOrderDto {
  id: string;
  pickupOption: PickupOption;
  containerSize: ContainerSize | null;
  dropOffDate: string | null;
  pickupDate: string | null;
  isHighPriority: boolean;
  collectingService: boolean;
  garbageOrderStatus: GarbageOrderStatus;
  cost: number;
  garbageGroupId: string;
  garbageGroupName: string | null;
  garbageGroupIsPrivate: boolean;
  assignedGarbageAdminId: string | null;
  assignedGarbageAdminUsername: string | null;
  assignedGarbageAdminAvatarName: string | null;
  utilizationFeeAmount: number | null;
  additionalUtilizationFeeAmount: number | null;
  utilizationProofBlobName: string | null;
  utilizationFeeSubmittedDateUtc: string | null;
  distanceInKilometers: number | null;
  users: GarbageAdminOrderUserDto[];
}
