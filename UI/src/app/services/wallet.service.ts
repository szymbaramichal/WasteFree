import { Injectable } from '@angular/core';
import { BehaviorSubject, of } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Result } from '../_models/result';
import { WalletBalanceDto, WalletMethodDto, WalletTransactionRequest, WalletTransactionResponse, PaymentStatus } from '../_models/wallet';

@Injectable({ providedIn: 'root' })
export class WalletService {
  private api = `${environment.apiUrl}/wallet`;

  private _balance$ = new BehaviorSubject<number>(0);
  balance$ = this._balance$.asObservable();

  private _methods$ = new BehaviorSubject<WalletMethodDto[] | null>(null);
  methods$ = this._methods$.asObservable();

  constructor(private http: HttpClient) {}

  // ----------- Public API -----------
  async ensureInit(force = false): Promise<void> {
    if (force || this._methods$.getValue() === null) {
      await Promise.allSettled([this.refreshMethods(), this.refreshBalance()]);
      return;
    }

    await this.refreshBalance();
  }

  async refreshBalance(): Promise<void> {
    this.http.post<Result<WalletBalanceDto>>(`${this.api}/balance`, {}).subscribe({
      next: (res) => {
        const value = res?.resultModel?.amount;
        if (typeof value === 'number' && !Number.isNaN(value)) {
          this._balance$.next(value);
        }
      }
    });
  }

  async refreshMethods(): Promise<void> {
    this.http.get<Result<WalletMethodDto[]>>(`${this.api}/methods`).subscribe({
      next: (res) => {
        if (Array.isArray(res?.resultModel)) this._methods$.next(res.resultModel); else this._methods$.next([]);
      },
      error: () => this._methods$.next([])
    });
  }

  resetState(): void {
    this._balance$.next(0);
    this._methods$.next(null);
  }

  get currentBalance(): number { return this._balance$.getValue(); }
  get currentMethods(): WalletMethodDto[] { return this._methods$.getValue() || []; }

  adjustBalance(delta: number): void {
    if (typeof delta !== 'number' || Number.isNaN(delta) || delta === 0) {
      return;
    }
    this._balance$.next(this._balance$.getValue() + delta);
  }

  // Creates a transaction (top up or withdraw) depending on code and amount sign
  createTransaction(req: WalletTransactionRequest) {
    return this.http.post<Result<WalletTransactionResponse>>(`${this.api}/transaction`, req).pipe(
      switchMap(res => {
        const raw = res?.resultModel?.paymentStatus as any;
        const numeric = typeof raw === 'string' ? Number(raw) : raw;
        const status: PaymentStatus = (numeric === PaymentStatus.Invalid || numeric === PaymentStatus.Pending || numeric === PaymentStatus.Completed)
          ? numeric
          : PaymentStatus.Pending;
        const base = { status, error: res?.errorMessage?.trim() ? res.errorMessage : undefined } as { status: PaymentStatus; error?: string };
        console.debug('[Wallet][Service] createTransaction raw response', { res, raw, numeric, mappedStatus: status, base });
        // Jeżeli sukces – pojedyncze odświeżenie salda (bez kalkulacji lokalnej)
        if (status === PaymentStatus.Completed) {
          this.refreshBalance();
        }
        return of(base);
      })
    );
  }
}
