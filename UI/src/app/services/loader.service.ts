import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class LoaderService {
  private isLoadingSubject = new BehaviorSubject<boolean>(false);
  isLoading$ = this.isLoadingSubject.asObservable();

  private startTime = 0;
  private minDuration = 0;
  private hideTimer: any = null;

  show(minDurationMs = 500) {
    // Cancel any pending hide to avoid flicker
    if (this.hideTimer) {
      clearTimeout(this.hideTimer);
      this.hideTimer = null;
    }
    this.startTime = Date.now();
    this.minDuration = Math.max(0, minDurationMs);
    if (!this.isLoadingSubject.value) {
      this.isLoadingSubject.next(true);
    }
  }

  hide() {
    const elapsed = Date.now() - this.startTime;
    const remaining = this.minDuration - elapsed;
    if (remaining <= 0) {
      this.isLoadingSubject.next(false);
    } else {
      this.hideTimer = setTimeout(() => {
        this.isLoadingSubject.next(false);
        this.hideTimer = null;
      }, remaining);
    }
  }
}
