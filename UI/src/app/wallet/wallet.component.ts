import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { WalletService } from '../services/wallet.service';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-wallet',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './wallet.component.html',
  styleUrls: ['./wallet.component.css']
})
export class WalletComponent implements OnInit, OnDestroy {
  balance = 0;
  topUpForm = this.fb.group({ amount: [10, [Validators.required, Validators.min(1)]] });
  withdrawForm = this.fb.group({ amount: [10, [Validators.required, Validators.min(1)]], iban: ['', [Validators.required]] });
  loading = false;
  message: string | null = null;

  constructor(private fb: FormBuilder, private wallet: WalletService) {
    this.balance = wallet.getBalance();
    this.wallet.balance$.subscribe(b => this.balance = b);
  }

  ngOnInit(): void {
    document.body.classList.add('portal-wallet-bg');
  }

  ngOnDestroy(): void {
    document.body.classList.remove('portal-wallet-bg');
  }

  async topUp() {
    if (this.topUpForm.invalid) return;
    this.loading = true; this.message = null;
    const amount = Number(this.topUpForm.value.amount);
    const res = await this.wallet.topUp(amount);
    this.loading = false;
    this.message = res.success ? 'Top up successful' : (res.message || 'Failed');
  }

  async withdraw() {
    if (this.withdrawForm.invalid) return;
    this.loading = true; this.message = null;
    const amount = Number(this.withdrawForm.value.amount);
    const iban = String(this.withdrawForm.value.iban || '');
    const res = await this.wallet.withdraw(amount, iban);
    this.loading = false;
    this.message = res.success ? 'Withdraw successful' : (res.message || 'Failed');
  }
}
