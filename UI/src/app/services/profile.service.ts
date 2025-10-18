import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Profile, ProfileUpdateRequest } from '@app/_models/profile';
import { Address } from '@app/_models/address';

@Injectable({ providedIn: 'root' })
export class ProfileService {
  private api = `${environment.apiUrl}/user/profile`;

  private _profile = signal<Profile | null>(null);
  profile = this._profile.asReadonly();

  private _loading = signal<boolean>(false);
  loading = this._loading.asReadonly();

  private _error = signal<string | null>(null);
  error = this._error.asReadonly();

  constructor(private http: HttpClient) {}

  refresh(): void {
    this._loading.set(true);
    this._error.set(null);
    this.http.get<any>(this.api).subscribe({
      next: (res) => {
  const dto: Profile | null = this.unwrap(res);
        this._profile.set(dto);
        this._loading.set(false);
      },
      error: (err) => {
        this._error.set('load_failed');
        this._loading.set(false);
        this._profile.set(null);
      }
    });
  }

  updateDescription(description: string) { return this.updateProfile({ description }); }

  updateProfile(payload: ProfileUpdateRequest) {

    const current = this._profile();
    const body: ProfileUpdateRequest = {
      description: payload.description !== undefined ? payload.description : current?.description,
      bankAccountNumber: payload.bankAccountNumber !== undefined ? payload.bankAccountNumber : current?.bankAccountNumber,
      address: this.buildAddressPayload(payload.address, current?.address)
    };

    return this.http.put<any>(this.api, body);
  }

  private unwrap(res: any): Profile | null {
    if (!res) return null;
    const raw = res?.resultModel ?? res; // support both wrapped and raw
    if (!raw) return null;
    const address = this.normalizeAddress(raw);
    return {
      userId: String(raw.userId ?? raw.UserId ?? ''),
      username: String(raw.username ?? raw.Username ?? ''),
      email: String(raw.email ?? raw.Email ?? ''),
      description: String(raw.description ?? raw.Description ?? ''),
      bankAccountNumber: String(raw.bankAccountNumber ?? raw.BankAccountNumber ?? ''),
      city: address.city,
      address
    };
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
}
