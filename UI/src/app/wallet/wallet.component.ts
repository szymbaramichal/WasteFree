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
  paymentStatus: PaymentStatus | null = null;

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
  }

  statusLabel(): string | null {
    switch (this.paymentStatus) {
      case PaymentStatus.Pending: return this.t.translate('wallet.payment.status.pending');
      case PaymentStatus.Success: return this.t.translate('wallet.payment.status.success');
      case PaymentStatus.Failed: return this.t.translate('wallet.payment.status.failed');
      default: return null;
    }
  }

  topUp() { this.handleTransaction('BLIK', this.topUpForm, { amount: 10, blikCode: '' }, 'wallet.message.topupSuccess'); }

  withdraw() { this.handleTransaction('IBAN', this.withdrawForm, { amount: 10, iban: '' }, 'wallet.message.withdrawSuccess'); }

  private handleTransaction(code: 'BLIK' | 'IBAN', form: any, resetValue: any, successKey: string) {
    if (form.invalid) return;
    this.resetMessages();
    this.loading = true;
    const amount = Number(form.value.amount);
    const paymentProperty = code === 'BLIK' ? String(form.value.blikCode) : String(form.value.iban);
    this.wallet.createTransaction({ code, amount, paymentProperty }).subscribe({
      next: ({ status, error }) => {
        this.paymentStatus = status;
        this.loading = false;
        if (status === PaymentStatus.Success) {
          this.message = this.t.translate(successKey);
          form.reset(resetValue);
        } else if (error) {
          this.error = error || this.t.translate('wallet.errors.api');
        }
      },
      error: (err) => {
        this.loading = false;
        this.paymentStatus = PaymentStatus.Failed;
        this.error = this.extractApiError(err) || this.t.translate('wallet.errors.api');
      }
    });
  }

  private resetMessages() {
    this.message = null;
    this.error = null;
    this.paymentStatus = null;
  }

  private extractApiError(err: any): string {
    const p = err?.error ?? err;
    if (!p) return '';
    if (typeof p === 'string') return p.trim();
    if (typeof p?.errorMessage === 'string' && p.errorMessage.trim()) return p.errorMessage.trim();
    const bag: any = (p && p.errors && typeof p.errors === 'object') ? p.errors : null;
    if (!bag) return '';
    try {
      return Object.values(bag)
        .flatMap((v: any) => Array.isArray(v) ? v : [v])
        .filter((m: any) => typeof m === 'string' && m.trim())
        .map((m: string) => m.trim())
        .filter((v, i, a) => a.indexOf(v) === i)
        .join('\n');
    } catch { return ''; }
  }
}
