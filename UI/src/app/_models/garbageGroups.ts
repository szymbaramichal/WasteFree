export interface GarbageGroup {
    id: string;
    name: string;
    description: string;
    users: GarbageGroupUser[];
};

export interface GarbageGroupUser {
    id: string; 
    username: string;
    garbageGroupRole: GarbageGroupRole;
    isPending: boolean;
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
}

export interface GarbageGroupInvitation {
        groupId: string;
        groupName: string;
        invitingUsername: string;
}