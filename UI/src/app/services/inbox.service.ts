import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Result, Pager } from '../_models/result';
import { Counter } from '../_models/inbox';

@Injectable({ providedIn: 'root' })
export class InboxService {
    private storageKey = 'wf_inbox_counter';
    private apiUrl = `${environment.apiUrl}`;

    counter = signal<number>(this.loadFromStorage());

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

    setCounter(counter: number) {
        this.counter.set(counter);
        localStorage.setItem(this.storageKey, counter.toString());
    }

    private loadFromStorage(): number {
        const raw = localStorage.getItem(this.storageKey);
        if(!raw) return 0;
        return Number(raw);
    }

    refreshCounter() {
        this.http.get<Result<Counter>>(this.apiUrl + '/inbox/counter').subscribe(count => this.counter.set(count.resultModel.unreadMessages));
    }

    fetchNotifications(pageNumber: number = 1, pageSize: number = 10) {
        this._loading.set(true);
        this._error.set(null);

        const params = `?pageNumber=${encodeURIComponent(pageNumber)}&pageSize=${encodeURIComponent(pageSize)}`;
        this.http.get<Result<NotificationItem[]>>(this.apiUrl + '/inbox/messages' + params)
            .subscribe({
                next: list => {
                    this._notifications.set(list.resultModel);
                    this._pager.set(list.pager ?? null);
                    this.setCounter(list.pager?.totalCount ?? 0);
                    this._loading.set(false);
                },
                error: err => {
                    this._error.set(err?.message ?? 'Unable to load notifications');
                    this._loading.set(false);
                }
            });
    }
}

export interface NotificationItem {
    id: string;
    title: string;
    body: string;
    createdDateUtc: string;
}
