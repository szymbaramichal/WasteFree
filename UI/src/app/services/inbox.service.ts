import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { PaginatedResult, Result, Pager } from '../_models/result';
import { Counter, NotificationItem } from '../_models/inbox';

@Injectable({ providedIn: 'root' })
export class InboxService {
    private storageKey = 'wf_inbox_counter';
    private apiUrl = `${environment.apiUrl}`;

    counter = signal<number>(this.loadFromStorage());
    private _lastPush = signal<number>(0);
    lastPush = this._lastPush.asReadonly();

    private _notifications = signal<NotificationItem[]>([]);
    notifications = this._notifications.asReadonly();
    private _loading = signal<boolean>(false);
    loading = this._loading.asReadonly();
    private _error = signal<string | null>(null);
    error = this._error.asReadonly();
    private _pager = signal<Pager | null>(null);
    pager = this._pager.asReadonly();

    constructor(private http: HttpClient) {
    }

    notifyPush() {
        this._lastPush.set(Date.now());
    }

    setCounter(counter: number) {
        const normalized = Number.isFinite(counter) ? counter : 0;
        const previous = this.counter();
        this.counter.set(normalized);
        localStorage.setItem(this.storageKey, normalized.toString());

        if (normalized > previous) {
            this._lastPush.set(Date.now());
        }
    }

    private loadFromStorage(): number {
        const raw = localStorage.getItem(this.storageKey);
        if(!raw) return 0;
        return Number(raw);
    }

    refreshCounter() {
        this.http.get<Result<Counter>>(this.apiUrl + '/inbox/counter').subscribe((count) => {
            const unread = count.resultModel?.unreadMessages ?? 0;
            this.setCounter(unread);
        });
    }

    fetchNotifications(pageNumber: number = 1, pageSize: number = 10) {
        this._loading.set(true);
        this._error.set(null);

        const params = `?pageNumber=${encodeURIComponent(pageNumber)}&pageSize=${encodeURIComponent(pageSize)}`;
        this.http.get<PaginatedResult<NotificationItem[]>>(this.apiUrl + '/inbox/messages' + params)
            .subscribe({
                next: list => {
                    const notifications = list.resultModel ?? [];
                    this._notifications.set(notifications);
                    this._pager.set(list.pager ?? null);
                    const total = list.pager?.totalCount ?? notifications.length;
                    this.setCounter(total);
                    this._loading.set(false);
                }
            });
    }

    makeAction(id: string, accept: boolean): Observable<Result<boolean>> {
        return this.http.post<Result<boolean>>(`${this.apiUrl}/inbox/messages/${id}/action/${accept}`, {});
    }

    deleteMessage(id: string): Observable<Result<boolean>> {
        return this.http.delete<Result<boolean>>(`${this.apiUrl}/inbox/${id}`);
    }

    clearAll(): Observable<Result<boolean>> {
        return this.http.post<Result<boolean>>(`${this.apiUrl}/inbox/clear`, {});
    }
}
