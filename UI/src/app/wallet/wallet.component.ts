import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { WalletService } from '../services/wallet.service';
import { PaymentStatus } from '../_models/wallet';
import { RouterModule } from '@angular/router';
import { TranslatePipe } from '../pipes/translate.pipe';
import { TranslationService } from '../services/translation.service';
import { Subscription } from 'rxjs';
import { ProfileService } from '../services/profile.service';

@Component({
  selector: 'app-wallet',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, TranslatePipe],
  templateUrl: './wallet.component.html',
  styleUrls: ['./wallet.component.css']
})

export class WalletComponent implements OnInit, OnDestroy {
  balance = 0;
  methodsLoaded = false;
  paymentStatus: PaymentStatus | null = null;
  PaymentStatus = PaymentStatus;

  topUpForm = this.fb.group({
    amount: [10, [Validators.required, Validators.min(1)]],
    blikCode: ['', [Validators.required, Validators.pattern(/^[0-9]{6}$/)]]
  });
  withdrawForm = this.fb.group({
    amount: [10, [Validators.required, Validators.min(1)]],
  });
  // Separate loading states so that top-up tile can show full-tile loader
  topUpLoading = false;
  withdrawLoading = false;
  // Backward compatibility: some template / code might still reference `loading`
  get loading(): boolean { return this.topUpLoading || this.withdrawLoading; }

  private balanceSub?: Subscription;

  profileSvc = inject(ProfileService);

  constructor(private fb: FormBuilder, private wallet: WalletService, private t: TranslationService) {
    this.balanceSub = this.wallet.balance$.subscribe(b => this.balance = b);
  }

  ngOnInit() {
    this.wallet.ensureInit().then(() => {
      this.methodsLoaded = true;
    });
    // ensure profile loaded to get saved bank account number
    this.profileSvc.refresh();
  }

  ngOnDestroy(): void {
    this.balanceSub?.unsubscribe();
  }


  topUp() {
    if (this.topUpForm.invalid) return;
    this.resetMessages();
    this.topUpLoading = true;
    const amount = Number(this.topUpForm.value.amount);
    const blik = String(this.topUpForm.value.blikCode);
    this.wallet.createTransaction({ code: 'BLIK', amount, paymentProperty: blik }).subscribe({
      next: ({ status, error }) => {
        this.paymentStatus = status;
        this.topUpLoading = false;
        if (status === PaymentStatus.Completed) {
          this.topUpForm.reset({ amount: 10, blikCode: '' });
        }
      },
      error: (err) => {
        this.topUpLoading = false;
        this.paymentStatus = PaymentStatus.Invalid;
      }
    });
  }

  withdraw() {
    if (this.withdrawForm.invalid) return;
    this.resetMessages();
    this.withdrawLoading = true;
    const amount = Number(this.withdrawForm.value.amount);
    const iban = this.profileSvc.profile()?.bankAccountNumber || '';
    if (!iban) {
      this.withdrawLoading = false;
      return;
    }
    this.wallet.createTransaction({ code: 'IBAN', amount, paymentProperty: iban }).subscribe({
      next: ({ status, error }) => {
        this.paymentStatus = status;
        this.withdrawLoading = false;
        if (status === PaymentStatus.Completed) {
          this.withdrawForm.reset({ amount: 10 });
        }
      },
      error: (err) => {
        this.withdrawLoading = false;
        this.paymentStatus = PaymentStatus.Invalid;
      }
    });
  }

  private resetMessages() {
    this.paymentStatus = null;
  }
}
