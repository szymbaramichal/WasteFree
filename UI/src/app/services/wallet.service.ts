import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class WalletService {
  private storageKey = 'wf_wallet_balance';
  private _balance$ = new BehaviorSubject<number>(this.load());
  balance$ = this._balance$.asObservable();

  constructor() {}

  private load(): number {
    try {
      const raw = localStorage.getItem(this.storageKey);
      if (!raw) return 0;
      const n = Number(raw);
      return isNaN(n) ? 0 : n;
    } catch {
      return 0;
    }
  }

  private save(v: number) {
    this._balance$.next(v);
    localStorage.setItem(this.storageKey, String(v));
  }

  getBalance(): number {
    return this._balance$.getValue();
  }

  // Mock top up via Blik: simply increases balance after a small delay
  async topUp(amount: number): Promise<{ success: boolean; message?: string }> {
    if (amount <= 0) return { success: false, message: 'Amount must be positive' };
    // simulate network / Blik flow delay
    await new Promise(r => setTimeout(r, 600));
    const newb = this.getBalance() + amount;
    this.save(Math.round(newb * 100) / 100);
    return { success: true };
  }

  // Withdraw after validating IBAN; amount must be <= balance
  async withdraw(amount: number, iban: string): Promise<{ success: boolean; message?: string }> {
    if (amount <= 0) return { success: false, message: 'Amount must be positive' };
    if (amount > this.getBalance()) return { success: false, message: 'Insufficient funds' };
    if (!this.validateIban(iban)) return { success: false, message: 'Invalid IBAN' };
    // simulate bank processing
    await new Promise(r => setTimeout(r, 600));
    const newb = this.getBalance() - amount;
    this.save(Math.round(newb * 100) / 100);
    return { success: true };
  }

  // Basic IBAN validation: remove spaces, check length & basic checksum using mod-97
  validateIban(iban: string): boolean {
    if (!iban) return false;
    // remove spaces and common separators and uppercase
    const normalized = iban.replace(/[^A-Za-z0-9]/g, '').toUpperCase();
    // IBAN length is country dependent but overall between 15 and 34
    if (normalized.length < 15 || normalized.length > 34) return false;

    // Move first four chars to the end
    const rearr = normalized.slice(4) + normalized.slice(0, 4);

    // Convert letters to numbers: A=10, B=11, ... Z=35
    let numeric = '';
    for (let i = 0; i < rearr.length; i++) {
      const ch = rearr.charAt(i);
      if (/[A-Z]/.test(ch)) {
        numeric += (ch.charCodeAt(0) - 55).toString();
      } else if (/[0-9]/.test(ch)) {
        numeric += ch;
      } else {
        return false; // unexpected char
      }
    }

    // Perform mod-97 using chunking to avoid big integers
    let remainder = 0;
    const chunkSize = 9; // safe chunk size for Number
    for (let offset = 0; offset < numeric.length; offset += chunkSize) {
      const chunk = numeric.substring(offset, offset + chunkSize);
      const num = Number(String(remainder) + chunk);
      remainder = num % 97;
    }

    return remainder === 1;
  }
}
