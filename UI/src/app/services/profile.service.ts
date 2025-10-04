import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

export interface ProfileDto {
  userId: string;
  username: string;
  email: string;
  description: string;
  bankAccountNumber: string;
  city: string;
}

@Injectable({ providedIn: 'root' })
export class ProfileService {
  private api = `${environment.apiUrl}/user/profile`;

  private _profile = signal<ProfileDto | null>(null);
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
        const dto: ProfileDto | null = this.unwrap(res);
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

  updateProfile(payload: { description?: string; bankAccountNumber?: string; city?: string }) {

    const current = this._profile();
    const body: any = {};
    if (payload.description !== undefined) {
      body.description = payload.description;
    } else if (current) {
      body.description = current.description;
    }

    if (payload.bankAccountNumber !== undefined) {
      body.bankAccountNumber = payload.bankAccountNumber;
    } else if (current) {
      body.bankAccountNumber = current.bankAccountNumber;
    }

    if (payload.city !== undefined) {
      body.city = payload.city;
    } else if (current) {
      body.city = current.city;
    }

    return this.http.put<any>(this.api, body);
  }

  private unwrap(res: any): ProfileDto | null {
    if (!res) return null;
    const raw = res?.resultModel ?? res; // support both wrapped and raw
    if (!raw) return null;
    return {
      userId: String(raw.userId ?? raw.UserId ?? ''),
      username: String(raw.username ?? raw.Username ?? ''),
      email: String(raw.email ?? raw.Email ?? ''),
      description: String(raw.description ?? raw.Description ?? ''),
      bankAccountNumber: String(raw.bankAccountNumber ?? raw.BankAccountNumber ?? ''),
      city: String(raw.city ?? raw.City ?? '')
    };
  }
}
