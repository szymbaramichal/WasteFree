import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface CurrentUser {
  nickname: string;
  role: 'User' | 'GarbageAdmin' | string;
}

@Injectable({
  providedIn: 'root'
})
export class CurrentUserService {
  private storageKey = 'wf_current_user';
  private _user$ = new BehaviorSubject<CurrentUser | null>(this.loadFromStorage());

  user$ = this._user$.asObservable();

  setUser(u: CurrentUser | null) {
    this._user$.next(u);
    if (u) {
      localStorage.setItem(this.storageKey, JSON.stringify(u));
    } else {
      localStorage.removeItem(this.storageKey);
    }
  }

  get snapshot(): CurrentUser | null {
    return this._user$.getValue();
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
