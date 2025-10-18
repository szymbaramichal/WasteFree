import { Address } from './address';

export type RegisterRole = 'User' | 'GarbageAdmin' | 'Admin';

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
  role: RegisterRole;
  languagePreference: string;
  address: Address;
}
