import { Address } from './address';

export interface GarbageGroup {
    id: string;
    name: string;
    description: string;
    city?: string | null;
    postalCode?: string | null;
    address: Address;
    users: GarbageGroupUser[];
};

export interface GarbageGroupUser {
    id: string;
    username: string;
    garbageGroupRole: GarbageGroupRole;
    isPending: boolean;
    avatarUrl?: string | null;
};

export interface GarbageGroupInfo {
    id: string;
    name: string;
    isUserOwner: boolean;
};

export enum GarbageGroupRole {
    Owner = 1,
    User
};

export interface RegisterGarbageGroupRequest {
    groupName: string;
    groupDescription: string;
    address: Address;
}

export interface UpdateGarbageGroupRequest {
    groupName: string;
    groupDescription: string;
    address: Address;
}

export interface GarbageGroupInvitation {
    groupId: string;
    groupName: string;
    invitingUsername: string;
}

export interface GarbageGroupWithUsers {
    groupId: string;
    groupName?: string | null;
    groupUsers: GarbageGroupUser[];
    isPrivate: boolean;
    address: Address;
}