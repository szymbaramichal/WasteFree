import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { WalletService } from '../services/wallet.service';
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
export class WalletComponent implements OnInit, OnDestroy {
  balance = 0;
  topUpForm = this.fb.group({ amount: [10, [Validators.required, Validators.min(1)]] });
  withdrawForm = this.fb.group({ amount: [10, [Validators.required, Validators.min(1)]], iban: ['', [Validators.required]] });
  loading = false;
  message: string | null = null;

  constructor(private fb: FormBuilder, private wallet: WalletService, private t: TranslationService) {
    this.balance = wallet.getBalance();
    this.wallet.balance$.subscribe(b => this.balance = b);
  }

  ngOnInit(): void {
    // Ensure the wallet page uses the same background image as /portal
    document.body.classList.add('portal-bg');
  }

  ngOnDestroy(): void {
    document.body.classList.remove('portal-bg');
  }

  async topUp() {
    if (this.topUpForm.invalid) return;
    this.loading = true; this.message = null;
    const amount = Number(this.topUpForm.value.amount);
    const res = await this.wallet.topUp(amount);
    this.loading = false;
    if (res.success) {
      this.message = this.t.translate('wallet.message.topupSuccess');
    } else {
      const msg = (res.message || '').toLowerCase();
      if (msg.includes('amount') && msg.includes('positive')) this.message = this.t.translate('wallet.errors.amountPositive');
      else this.message = this.t.translate('wallet.errors.unknown');
    }
  }

  async withdraw() {
    if (this.withdrawForm.invalid) return;
    this.loading = true; this.message = null;
    const amount = Number(this.withdrawForm.value.amount);
    const iban = String(this.withdrawForm.value.iban || '');
    const res = await this.wallet.withdraw(amount, iban);
    this.loading = false;
    if (res.success) {
      this.message = this.t.translate('wallet.message.withdrawSuccess');
    } else {
      const msg = (res.message || '').toLowerCase();
      if (msg.includes('amount') && msg.includes('positive')) this.message = this.t.translate('wallet.errors.amountPositive');
      else if (msg.includes('insufficient')) this.message = this.t.translate('wallet.errors.insufficientFunds');
      else if (msg.includes('iban')) this.message = this.t.translate('wallet.errors.invalidIban');
      else this.message = this.t.translate('wallet.errors.unknown');
    }
  }
}
