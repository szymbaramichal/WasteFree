import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, of } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class InboxService {
    private storageKey = 'wf_inbox_counter';
    private apiUrl = `${environment.apiUrl}`;

    private _counter = signal<number>(this.loadFromStorage());
    counter = this._counter.asReadonly();

    // notifications list
    private _notifications = signal<NotificationItem[]>([]);
    notifications = this._notifications.asReadonly();
    private _loading = signal<boolean>(false);
    loading = this._loading.asReadonly();
    private _error = signal<string | null>(null);
    error = this._error.asReadonly();

    constructor(private http: HttpClient) {
        this.refresh();
    }

    setCounter(counter: number) {
        this._counter.set(counter);
        localStorage.setItem(this.storageKey, counter.toString());
    }

    private loadFromStorage(): number {
        const raw = localStorage.getItem(this.storageKey);
        if(!raw) return 0;
        return Number(raw);
    }

    refresh() {
        this.http.get<number>(this.apiUrl + '/inbox/counter').pipe(
            catchError(() => of(0))
        ).subscribe(count => this._counter.set(count));
    }

    fetchNotifications() {
        this._loading.set(true);
        this._error.set(null);
        this.http.get<NotificationItem[]>(this.apiUrl + '/inbox/list').pipe(
            catchError(err => {
                this._error.set('load_failed');
                return of([] as NotificationItem[]);
            })
        ).subscribe(list => {
            // sort newest first by createdAt if exists
            const sorted = [...list].sort((a,b) => {
                const da = new Date(a.createdAt || 0).getTime();
                const db = new Date(b.createdAt || 0).getTime();
                return db - da;
            });
            this._notifications.set(sorted);
            this._loading.set(false);
            // update counter with unread items
            const unread = sorted.filter(n => !n.read).length;
            this.setCounter(unread);
        });
    }

    markAsRead(id: string) {
        const list = this._notifications();
        const idx = list.findIndex(n => n.id === id);
        if (idx === -1) return;
        // optimistic update
        const updated = [...list];
        if (!updated[idx].read) {
            updated[idx] = { ...updated[idx], read: true };
            this._notifications.set(updated);
            this.setCounter(updated.filter(n => !n.read).length);
        }
        this.http.post(this.apiUrl + '/inbox/mark-read', { id }).pipe(catchError(() => of(null))).subscribe();
    }

    markAllRead() {
        const list = this._notifications();
        if (!list.some(n => !n.read)) return;
        const updated = list.map(n => ({ ...n, read: true }));
        this._notifications.set(updated);
        this.setCounter(0);
        this.http.post(this.apiUrl + '/inbox/mark-all-read', {}).pipe(catchError(() => of(null))).subscribe();
    }
}

export interface NotificationItem {
    id: string;
    type: 'info' | 'warning' | 'success' | string;
    title: string;
    message: string;
    createdAt?: string;
    read?: boolean;
}
