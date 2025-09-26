import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, of } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class InboxService {
    private storageKey = 'wf_inbox_counter';
    private apiUrl = `${environment.apiUrl}/auth`;

    private _counter = signal<number>(this.loadFromStorage());
    counter = this._counter.asReadonly();

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
}
