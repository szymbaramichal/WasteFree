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
import { ToastrService } from 'ngx-toastr';
import { extractApiErrorPayload } from '../helpers/api-error.helper';

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
  toastr = inject(ToastrService);

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
  message: string | null = null;
  error: string | null = null;

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

  statusLabel(): string | null {
    if (this.paymentStatus === null) return null;
    switch (this.paymentStatus) {
      case PaymentStatus.Pending: return this.t.translate('wallet.payment.status.pending');
      case PaymentStatus.Completed: return this.t.translate('wallet.payment.status.success');
      case PaymentStatus.Invalid: return this.t.translate('wallet.payment.status.failed');
      default: return null;
    }
  }

  // Unified alert text preference order:
  // 1. Explicit error
  // 2. Success message (message)
  // 3. Status label (pending etc.)
  get alertText(): string | null {
    if (this.error) return this.error;
    if (this.message) return this.message;
    return this.statusLabel();
  }

  get alertType(): 'info' | 'success' | 'danger' | null {
    if (!this.alertText) return null;
    if (this.error) return 'danger';
    if (this.paymentStatus === PaymentStatus.Completed) return 'success';
    if (this.paymentStatus === PaymentStatus.Pending) return 'info';
    if (this.paymentStatus === PaymentStatus.Invalid) return 'danger';
    return 'info';
  }

  // Extended presentation helpers
  get alertTitle(): string | null {
    if (!this.alertType) return null;
    const map: Record<string, { key: string; fallback: string }> = {
      success: { key: 'wallet.alert.successTitle', fallback: 'Sukces' },
      danger: { key: 'wallet.alert.errorTitle', fallback: 'Błąd' },
      info: { key: 'wallet.alert.infoTitle', fallback: 'Informacja' }
    };
    const def = map[this.alertType];
    if (!def) return null;
    const translated = this.t.translate(def.key);
    // If translation service returns the key itself or empty string, use fallback.
    if (!translated || translated === def.key) return def.fallback;
    return translated;
  }

  alertVisible = true;
  private autoDismissTimer: any = null;
  private autoDismissMs = 5500; // configurable

  dismissAlert() {
    this.alertVisible = false;
    if (this.autoDismissTimer) {
      clearTimeout(this.autoDismissTimer);
      this.autoDismissTimer = null;
    }
  }

  private scheduleAutoDismiss() {
    if (this.autoDismissTimer) {
      clearTimeout(this.autoDismissTimer);
    }
    if (this.alertType === 'success') {
      this.autoDismissTimer = setTimeout(() => {
        this.alertVisible = false;
      }, this.autoDismissMs);
    }
  }

  topUp() {
    this.toastr.success('test');
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
          this.message = this.t.translate('wallet.message.topupSuccess');
          this.topUpForm.reset({ amount: 10, blikCode: '' });
          this.alertVisible = true;
          this.scheduleAutoDismiss();
        } else if (error) {
          this.error = error; 
          this.alertVisible = true;
        }
      },
      error: (err) => {
        this.topUpLoading = false;
        this.paymentStatus = PaymentStatus.Invalid;
        this.error = this.extractApiError(err) || this.t.translate('wallet.errors.api');
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
      this.error = this.t.translate('wallet.withdraw.noIban');
      return;
    }
    this.wallet.createTransaction({ code: 'IBAN', amount, paymentProperty: iban }).subscribe({
      next: ({ status, error }) => {
        this.paymentStatus = status;
        this.withdrawLoading = false;
        if (status === PaymentStatus.Completed) {
          this.message = this.t.translate('wallet.message.withdrawSuccess');
          this.withdrawForm.reset({ amount: 10 });
          this.alertVisible = true;
          this.scheduleAutoDismiss();
        } else if (error) {
          this.error = error;
          this.alertVisible = true;
        }
      },
      error: (err) => {
        this.withdrawLoading = false;
        this.paymentStatus = PaymentStatus.Invalid;
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
    return extractApiErrorPayload(err?.error ?? err);
  }
}
