export interface Counter {
    unreadMessages: number;
}

export enum InboxActionType {
    None = 0,
    GroupInvitation = 1,
    MakePayment = 2,
    GarbageOrderDetails = 3
}

export interface NotificationItem {
    id: string;
    title: string;
    body: string;
    createdDateUtc: string;
    actionType?: InboxActionType | string | null;
    relatedEntityId?: string | null;
}