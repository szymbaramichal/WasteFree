import { Injectable, signal } from '@angular/core';
import { CurrentUser } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class CurrentUserService {
  private storageKey = 'wf_current_user';
  private avatarPrefix = 'wf_avatar_';
  private _user = signal<CurrentUser | null>(this.loadFromStorage());
  user = this._user.asReadonly();

  setUser(u: CurrentUser | null) {
    const previous = this._user();
    if (u) {
      const cachedAvatar = this.restoreCachedAvatar(u.id);
      const incomingAvatar = typeof u.avatarUrl === 'string' ? u.avatarUrl.trim() : u.avatarUrl ?? null;
      const avatarUrl = incomingAvatar && incomingAvatar.length > 0 ? incomingAvatar : cachedAvatar ?? null;
      const normalized: CurrentUser = { ...u, avatarUrl };
      this._user.set(normalized);
      localStorage.setItem(this.storageKey, JSON.stringify(normalized));
      if (avatarUrl) {
        localStorage.setItem(this.avatarCacheKey(normalized.id), avatarUrl);
      } else {
        localStorage.removeItem(this.avatarCacheKey(normalized.id));
      }
    } else {
      this._user.set(null);
      localStorage.removeItem(this.storageKey);
      if (previous?.id) {
        localStorage.removeItem(this.avatarCacheKey(previous.id));
      }
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
      const incomingAvatar = typeof parsed.avatarUrl === 'string' ? parsed.avatarUrl.trim() : parsed.avatarUrl ?? null;
      if (!incomingAvatar) {
        const cached = this.restoreCachedAvatar(parsed.id);
        if (cached) {
          parsed.avatarUrl = cached;
        }
      } else {
        parsed.avatarUrl = incomingAvatar;
        localStorage.setItem(this.avatarCacheKey(parsed.id), incomingAvatar);
      }
      return parsed;
    } catch {
      return null;
    }
  }

  private restoreCachedAvatar(userId: string): string | null {
    try {
      return localStorage.getItem(this.avatarCacheKey(userId)) ?? null;
    } catch {
      return null;
    }
  }

  private avatarCacheKey(userId: string): string {
    return `${this.avatarPrefix}${userId}`;
  }
}
