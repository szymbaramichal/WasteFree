import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { WalletService } from '../services/wallet.service';
import { PaymentStatus } from '../_models/wallet';
import { RouterModule } from '@angular/router';
import { TranslatePipe } from '../pipes/translate.pipe';
import { TranslationService } from '../services/translation.service';

@Component({
  selector: 'app-wallet',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, TranslatePipe],
  templateUrl: './wallet.component.html',
  styleUrls: ['./wallet.component.css']
})
export class WalletComponent {
  balance = 0;
  methodsLoaded = false;
  paymentStatus: PaymentStatus | null = null;
  PaymentStatus = PaymentStatus; // expose enum for template

  topUpForm = this.fb.group({
    amount: [10, [Validators.required, Validators.min(1)]],
    blikCode: ['', [Validators.required, Validators.pattern(/^[0-9]{6}$/)]]
  });
  withdrawForm = this.fb.group({
    amount: [10, [Validators.required, Validators.min(1)]],
    iban: ['', [Validators.required]]
  });
  loading = false;
  message: string | null = null;
  error: string | null = null;

  constructor(private fb: FormBuilder, private wallet: WalletService, private t: TranslationService) {
    this.wallet.balance$.subscribe(b => this.balance = b);
  }

  async ngOnInit() {
    await this.wallet.ensureInit();
    this.balance = this.wallet.currentBalance;
    this.methodsLoaded = true;
  }

  statusLabel(): string | null {
    if (this.paymentStatus === null) return null;
    switch (this.paymentStatus) {
      case PaymentStatus.Pending: return this.t.translate('wallet.payment.status.pending');
      case PaymentStatus.Success: return this.t.translate('wallet.payment.status.success');
      case PaymentStatus.Failed: return this.t.translate('wallet.payment.status.failed');
      default: return null;
    }
  }

  topUp() {
    if (this.topUpForm.invalid) return;
    this.resetMessages();
    this.loading = true;
    const amount = Number(this.topUpForm.value.amount);
    const blik = String(this.topUpForm.value.blikCode);
    this.wallet.createTransaction({ code: 'BLIK', amount, paymentProperty: blik }).subscribe({
      next: ({ status, error }) => {
        this.paymentStatus = status;
        this.loading = false;
        if (status === PaymentStatus.Success) {
          this.message = this.t.translate('wallet.message.topupSuccess');
          this.topUpForm.reset({ amount: 10, blikCode: '' });
        } else if (error) {
          this.error = this.mapError(error);
        }
      },
      error: (err) => {
        this.loading = false;
        this.paymentStatus = PaymentStatus.Failed;
        this.error = this.extractApiError(err) || this.t.translate('wallet.errors.api');
      }
    });
  }

  withdraw() {
    if (this.withdrawForm.invalid) return;
    this.resetMessages();
    this.loading = true;
    const amount = Number(this.withdrawForm.value.amount);
    const iban = String(this.withdrawForm.value.iban);
    this.wallet.createTransaction({ code: 'IBAN', amount, paymentProperty: iban }).subscribe({
      next: ({ status, error }) => {
        this.paymentStatus = status;
        this.loading = false;
        if (status === PaymentStatus.Success) {
          this.message = this.t.translate('wallet.message.withdrawSuccess');
          this.withdrawForm.reset({ amount: 10, iban: '' });
        } else if (error) {
          this.error = this.mapError(error);
        }
      },
      error: (err) => {
        this.loading = false;
        this.paymentStatus = PaymentStatus.Failed;
        this.error = this.extractApiError(err) || this.t.translate('wallet.errors.api');
      }
    });
  }

  private mapError(raw: string): string {
    const lower = raw.toLowerCase();
    if (lower.includes('insufficient')) return this.t.translate('wallet.errors.insufficientFunds');
    if (lower.includes('iban')) return this.t.translate('wallet.errors.invalidIban');
    if (lower.includes('amount') && lower.includes('positive')) return this.t.translate('wallet.errors.amountPositive');
    return this.t.translate('wallet.errors.api');
  }

  private resetMessages() {
    this.message = null;
    this.error = null;
    this.paymentStatus = null;
  }

  // Zbliżone do wzorca w auth.component.ts
  private extractApiError(err: any): string {
    const p = err?.error ?? err;
    if (!p) return '';
    if (typeof p === 'string') return p.trim();
    if (typeof p?.errorMessage === 'string' && p.errorMessage.trim()) return p.errorMessage.trim();
    const bag: any = (p && typeof p === 'object' && p.errors && typeof p.errors === 'object') ? p.errors : p;
    try {
      const values = Object.values(bag as Record<string, unknown>);
      const messages = values
        .flatMap((v: any) => Array.isArray(v) ? v : [v])
        .filter((m: any) => typeof m === 'string' && m.trim())
        .map((m: string) => m.trim());
      return Array.from(new Set(messages)).join('\n');
    } catch { return ''; }
  }
}
