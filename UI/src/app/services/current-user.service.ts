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
    this._user.set(u);
    if (u) {
      localStorage.setItem(this.storageKey, JSON.stringify(u));
    } else {
      localStorage.removeItem(this.storageKey);
    }
  }

  private loadFromStorage(): CurrentUser | null {
    try {
      const raw = localStorage.getItem(this.storageKey);
      if (!raw) return null;
      return JSON.parse(raw) as CurrentUser;
    } catch {
      return null;
    }
  }
}
