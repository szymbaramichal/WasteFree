import { Injectable, signal } from '@angular/core';
import { CurrentUser } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class CurrentUserService {
  private storageKey = 'wf_current_user';
  private _user = signal<CurrentUser | null>(this.loadFromStorage());
  user = this._user.asReadonly();

  setUser(u: CurrentUser | null) {
    if (u) {
      const avatarUrl = this.normalizeAvatar(u.avatarUrl);
      const normalized: CurrentUser = { ...u, avatarUrl };
      this._user.set(normalized);
      localStorage.setItem(this.storageKey, JSON.stringify(normalized));
    } else {
      this._user.set(null);
      localStorage.removeItem(this.storageKey);
    }
  }

  private loadFromStorage(): CurrentUser | null {
    try {
      const raw = localStorage.getItem(this.storageKey);
      if (!raw) return null;
      const parsed = JSON.parse(raw) as CurrentUser;
      if (!parsed?.id) {
        return parsed;
      }
      parsed.avatarUrl = this.normalizeAvatar(parsed.avatarUrl);
      return parsed;
    } catch {
      return null;
    }
  }

  private normalizeAvatar(value: unknown): string | null {
    if (typeof value !== 'string') {
      return null;
    }
    const trimmed = value.trim();
    return trimmed.length > 0 ? trimmed : null;
  }
}
