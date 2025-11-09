export interface MyPickupParticipantDto {
  userId: string;
  hasAcceptedPayment: boolean;
  shareAmount: number;
}

export interface MyPickupDto {
  id: string;
  pickupOption: number;
  containerSize: number | null;
  dropOffDate: string | null;
  pickupDate: string | null;
  isHighPriority: boolean;
  collectingService: boolean;
  garbageOrderStatus: number;
  cost: number;
  garbageGroupId: string | null;
  garbageGroupName: string | null;
  garbageGroupIsPrivate: boolean;
  users: MyPickupParticipantDto[];
}
