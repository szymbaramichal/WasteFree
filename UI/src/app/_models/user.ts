export interface User {
    id: string;
    email: string;
    username: string;
    token?: string;
    role: string;
};

export interface CurrentUser {
  id: string;
  username: string;
  role: UserRole;
}

export enum UserRole {
  User = 'User',
  GarbageAdmin = 'GarbageAdmin',
  Admin = 'Admin',
}