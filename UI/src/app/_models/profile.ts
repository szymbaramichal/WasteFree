import { Address } from './address';

export type PickupOptionKey = 'smallPickup' | 'pickup' | 'container' | 'specialOrder';

export interface Profile {
  userId: string;
  username: string;
  email: string;
  description: string;
  bankAccountNumber: string;
  city: string;
  address: Address;
  avatarUrl: string | null;
  pickupOptions: PickupOptionKey[];
}

export interface ProfileUpdateRequest {
  description?: string | null;
  bankAccountNumber?: string | null;
  address?: Address;
  pickupOptions?: PickupOptionKey[] | null;
}
