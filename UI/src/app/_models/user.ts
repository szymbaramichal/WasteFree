export interface User {
    id: string;
    email: string;
    username: string;
    token?: string;
    userRole: UserRole;
    acceptedConsents: boolean;
};

export interface CurrentUser {
  id: string;
  username: string;
  role: UserRole;
  acceptedConsents?: boolean;
}

export enum UserRole {
  User = 1,
  GarbageAdmin = 2,
  Admin = 3,
}