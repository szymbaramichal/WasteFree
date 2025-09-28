import { Injectable } from '@angular/core';
import { BehaviorSubject, firstValueFrom, timer } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Result } from '../_models/result';

export interface WalletBalanceDto { amount: number; }
export interface WalletMethodDto { code: string; name: string; type: number; }
export interface WalletTransactionRequest { code: string; amount: number; paymentProperty: string; }
export interface WalletTransactionResponse { paymentStatus: number; }

// Map numeric paymentStatus -> semantic value (assumption based on typical flows):
// 0 = Pending, 1 = Failed, 2 = Success (given example shows 2 on success)
export enum PaymentStatus { Pending = 0, Failed = 1, Success = 2 }

@Injectable({ providedIn: 'root' })
export class WalletService {
  private api = `${environment.apiUrl}/wallet`;

  private _balance$ = new BehaviorSubject<number>(0);
  balance$ = this._balance$.asObservable();

  private _methods$ = new BehaviorSubject<WalletMethodDto[] | null>(null);
  methods$ = this._methods$.asObservable();

  constructor(private http: HttpClient) {}

  // ----------- Public API -----------
  async ensureInit(): Promise<void> {
    // Lazy load methods & balance only once
    if (this._methods$.getValue() === null) {
      await Promise.allSettled([this.refreshMethods(), this.refreshBalance()]);
    }
  }

  async refreshBalance(): Promise<void> {
    try {
      const res = await firstValueFrom(this.http.get<Result<WalletBalanceDto>>(`${this.api}/balance`));
      if (res?.resultModel && typeof res.resultModel.amount === 'number') {
        this._balance$.next(res.resultModel.amount);
      }
    } catch {
      // silent; keep previous value
    }
  }

  async refreshMethods(): Promise<void> {
    try {
      const res = await firstValueFrom(this.http.get<Result<WalletMethodDto[]>>(`${this.api}/methods`));
      if (Array.isArray(res?.resultModel)) this._methods$.next(res.resultModel);
      else this._methods$.next([]);
    } catch {
      this._methods$.next([]);
    }
  }

  get currentBalance(): number { return this._balance$.getValue(); }
  get currentMethods(): WalletMethodDto[] { return this._methods$.getValue() || []; }

  // Creates a transaction (top up or withdraw) depending on code and amount sign
  async createTransaction(req: WalletTransactionRequest): Promise<{ status: PaymentStatus; error?: string }> {
    const before = this.currentBalance;
    try {
      const res = await firstValueFrom(this.http.post<Result<WalletTransactionResponse>>(`${this.api}/transaction`, req));
      const status = res?.resultModel?.paymentStatus ?? PaymentStatus.Pending;

      if (status === PaymentStatus.Success) {
        // Poll backend a few times (e.g. eventual consistency) up to 2s total
        await this.pollBalanceWithFallback(req, before);
      }
      if (res?.errorMessage) return { status, error: res.errorMessage };
      return { status };
    } catch (e: any) {
      const errMsg = (e?.error?.errorMessage || e?.message || '').toString();
      return { status: PaymentStatus.Failed, error: errMsg || 'API_ERROR' };
    }
  }

  private async pollBalanceWithFallback(req: WalletTransactionRequest, before: number) {
    const expected = this.expectedBalance(afterSign(req, before));
    for (let attempt = 0; attempt < 4; attempt++) {
      await this.refreshBalance();
      if (almostEqual(this.currentBalance, expected)) return; // updated as expected
      await wait(500);
    }
    // Fallback: if backend still returns old balance, apply optimistic local adjustment
    if (this.currentBalance === before) {
      this._balance$.next(this.expectedBalance(afterSign(req, before)));
    }
  }

  private expectedBalance(val: number) { return Math.round(val * 100) / 100; }
}

function afterSign(req: WalletTransactionRequest, before: number): number {
  // Założenie: BLIK = top-up (add), IBAN = withdraw (subtract)
  if (req.code === 'BLIK') return before + req.amount;
  if (req.code === 'IBAN') return before - req.amount;
  return before;
}

function almostEqual(a: number, b: number, eps = 0.005) { return Math.abs(a - b) < eps; }
function wait(ms: number) { return new Promise(r => setTimeout(r, ms)); }
