export interface Counter {
    unreadMessages: number;
}

export interface NotificationItem {
    id: string;
    title: string;
    body: string;
    createdDateUtc: string;
}