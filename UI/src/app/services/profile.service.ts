import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { PickupOptionKey, Profile, ProfileUpdateRequest } from '@app/_models/profile';
import { Address } from '@app/_models/address';
import { CurrentUserService } from '@app/services/current-user.service';
import { tap } from 'rxjs';

const PICKUP_OPTION_VALUE_BY_KEY: Record<PickupOptionKey, number> = {
  smallPickup: 0,
  pickup: 1,
  container: 2,
  specialOrder: 3
};

const PICKUP_OPTION_KEY_BY_VALUE = new Map<number, PickupOptionKey>(
  Object.entries(PICKUP_OPTION_VALUE_BY_KEY).map(([key, value]) => [value, key as PickupOptionKey])
);

const PICKUP_OPTION_KEY_BY_NAME = new Map<string, PickupOptionKey>(
  (Object.keys(PICKUP_OPTION_VALUE_BY_KEY) as PickupOptionKey[]).map(key => [key.toLowerCase(), key] as const)
);

interface ApiProfileUpdateRequest {
  description: string;
  bankAccountNumber: string;
  address: Address;
  pickupOptions: number[];
}

@Injectable({ providedIn: 'root' })
export class ProfileService {
  private api = `${environment.apiUrl}/user/profile`;

  private _profile = signal<Profile | null>(null);
  profile = this._profile.asReadonly();

  private _loading = signal<boolean>(false);
  loading = this._loading.asReadonly();


  constructor(private http: HttpClient, private currentUser: CurrentUserService) {}

  refresh(): void {
    this._loading.set(true);
    this.http.get<any>(this.api).subscribe({
      next: (res) => {
        const dto: Profile | null = this.unwrap(res);
        this._profile.set(dto);
        if (dto) {
          const existing = this.currentUser.user();
          if (existing && existing.id === dto.userId) {
            this.currentUser.setUser({
              ...existing,
              username: dto.username || existing.username
            });
          }
        }
        this._loading.set(false);
      },
      error: () => {
        this._loading.set(false);
        this._profile.set(null);
      }
    });
  }

  clear(): void {
    this._loading.set(false);
    this._profile.set(null);
  }

  updateDescription(description: string) { return this.updateProfile({ description }); }

  updateProfile(payload: ProfileUpdateRequest) {
    const current = this._profile();
    const pickupSelection = payload.pickupOptions === undefined
      ? current?.pickupOptions ?? []
      : payload.pickupOptions ?? [];

    const body: ApiProfileUpdateRequest = {
      description: this.resolveString(payload.description, current?.description),
      bankAccountNumber: this.resolveString(payload.bankAccountNumber, current?.bankAccountNumber),
      address: this.buildAddressPayload(payload.address, current?.address),
      pickupOptions: this.toApiPickupOptions(pickupSelection)
    };

    return this.http.put<any>(this.api, body);
  }

  uploadAvatar(file: File) {
    const endpoint = `${environment.apiUrl}/user/avatar/upload`;
    const formData = new FormData();
    formData.append('avatar', file);
    return this.http.post<any>(endpoint, formData).pipe(
      tap(res => {
        const resolved = this.extractAvatarUrl(res);
        if (!resolved) {
          return;
        }
        const avatarUrl = this.applyCacheBuster(resolved);
        const currentProfile = this._profile();
        if (currentProfile) {
          this._profile.set({ ...currentProfile, avatarUrl });
        }
        const existing = this.currentUser.user();
        if (existing) {
          this.currentUser.setUser({ ...existing, avatarUrl });
        }
      })
    );
  }

  private unwrap(res: any): Profile | null {
    if (!res) return null;
    const raw = res?.resultModel ?? res; // support both wrapped and raw
    if (!raw) return null;
    const address = this.normalizeAddress(raw);
    const avatarRaw = raw.avatarUrl ?? raw.AvatarUrl ?? null;
    const avatarUrl = this.normalizeAvatar(avatarRaw);
    return {
      userId: String(raw.userId ?? raw.UserId ?? ''),
      username: String(raw.username ?? raw.Username ?? ''),
      email: String(raw.email ?? raw.Email ?? ''),
      description: String(raw.description ?? raw.Description ?? ''),
      bankAccountNumber: String(raw.bankAccountNumber ?? raw.BankAccountNumber ?? ''),
      city: address.city,
      address,
      avatarUrl,
      pickupOptions: this.normalizePickupOptions(raw.pickupOptions ?? raw.PickupOptions)
    };
  }

  private extractAvatarUrl(res: any): string | null {
    const raw = res?.resultModel ?? res ?? {};
    return this.normalizeAvatar(raw.avatarUrl ?? raw.AvatarUrl ?? null);
  }

  private applyCacheBuster(url: string): string {
    try {
      const base = typeof window !== 'undefined' ? window.location.origin : undefined;
      const parsed = new URL(url, base);
      parsed.searchParams.delete('v');
      parsed.searchParams.set('v', Date.now().toString());
      return parsed.toString();
    } catch {
      const sep = url.includes('?') ? '&' : '?';
      return `${url}${sep}v=${Date.now()}`;
    }
  }

  private normalizeAvatar(value: unknown): string | null {
    if (typeof value !== 'string') {
      return null;
    }
    const trimmed = value.trim();
    return trimmed.length > 0 ? trimmed : null;
  }

  private normalizeAddress(raw: any): Address {
    const address = raw?.address ?? raw?.Address ?? {};
    const city = address?.city ?? raw?.city ?? raw?.City ?? '';
    const postalCode = address?.postalCode ?? raw?.postalCode ?? raw?.PostalCode ?? '';
    const street = address?.street ?? raw?.street ?? raw?.Street ?? '';
    return {
      city: String(city ?? ''),
      postalCode: String(postalCode ?? ''),
      street: String(street ?? '')
    };
  }

  private buildAddressPayload(incoming: Address | undefined, current: Address | undefined): Address {
    const base = incoming ?? current ?? { city: '', postalCode: '', street: '' };
    return {
      city: String(base.city ?? '').trim(),
      postalCode: String(base.postalCode ?? '').trim(),
      street: String(base.street ?? '').trim()
    };
  }

  private resolveString(value: string | null | undefined, fallback: string | undefined): string {
    if (value !== undefined) {
      return value ?? '';
    }
    return fallback ?? '';
  }

  private normalizePickupOptions(raw: unknown): PickupOptionKey[] {
    if (!Array.isArray(raw)) {
      return [];
    }

    const result: PickupOptionKey[] = [];
    const seen = new Set<PickupOptionKey>();

    for (const entry of raw) {
      const mapped = this.mapPickupOption(entry);
      if (mapped && !seen.has(mapped)) {
        seen.add(mapped);
        result.push(mapped);
      }
    }

    return result;
  }

  private mapPickupOption(value: unknown): PickupOptionKey | null {
    if (typeof value === 'number') {
      return PICKUP_OPTION_KEY_BY_VALUE.get(value) ?? null;
    }

    if (typeof value === 'string') {
      const trimmed = value.trim();
      if (!trimmed) {
        return null;
      }

      const numeric = Number(trimmed);
      if (!Number.isNaN(numeric)) {
        const numericMatch = PICKUP_OPTION_KEY_BY_VALUE.get(numeric);
        if (numericMatch) {
          return numericMatch;
        }
      }

      return PICKUP_OPTION_KEY_BY_NAME.get(trimmed.toLowerCase()) ?? null;
    }

    return null;
  }

  private toApiPickupOptions(values: PickupOptionKey[] | null | undefined): number[] {
    if (!values || values.length === 0) {
      return [];
    }

    const result: number[] = [];
    const seen = new Set<number>();

    for (const key of values) {
      const numeric = PICKUP_OPTION_VALUE_BY_KEY[key];
      if (numeric === undefined || seen.has(numeric)) {
        continue;
      }
      seen.add(numeric);
      result.push(numeric);
    }

    return result;
  }
}
