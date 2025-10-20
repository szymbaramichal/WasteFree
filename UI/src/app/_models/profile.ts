import { Address } from './address';

export interface Profile {
  userId: string;
  username: string;
  email: string;
  description: string;
  bankAccountNumber: string;
  city: string;
  address: Address;
  avatarUrl: string | null;
}

export interface ProfileUpdateRequest {
  description?: string | null;
  bankAccountNumber?: string | null;
  address?: Address;
}
