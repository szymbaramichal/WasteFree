export interface User {
    id: string;
    email: string;
    username: string;
    token?: string;
    userRole: UserRole;
    acceptedConsents: boolean;
    avatarUrl?: string | null;
};

export interface CurrentUser {
  id: string;
  username: string;
  role: UserRole;
  acceptedConsents?: boolean;
  avatarUrl?: string | null;
}

export enum UserRole {
  User = 1,
  GarbageAdmin = 2,
  Admin = 3,
}

export interface UserStats {
  savings: number;
  wasteReduced: number;
  collections: number;
  communityCount: number;
}