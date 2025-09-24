import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { TranslatePipe } from '../pipes/translate.pipe';
import { TranslationService } from '../services/translation.service';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-activation',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslatePipe],
  template: `
  <div class="activation-shell">
    <div class="activation-box">
      <h2 *ngIf="status === 'pending'">{{ 'auth.account.activating' | translate }}</h2>
      <h2 *ngIf="status === 'success'">{{ 'auth.account.activated.title' | translate }}</h2>
      <h2 *ngIf="status === 'error'">{{ 'auth.account.activated.errorTitle' | translate }}</h2>

      <p *ngIf="status === 'pending'">{{ 'auth.account.activated.wait' | translate }}</p>
      <p *ngIf="status === 'success'">{{ 'auth.account.activated.success' | translate }}</p>
      <p *ngIf="status === 'error'">{{ message || ('auth.account.activated.error' | translate) }}</p>

      <div class="actions">
        <button *ngIf="status === 'success'" class="grow" (click)="gotoLogin()">{{ 'auth.backToLogin' | translate }}</button>
        <button *ngIf="status === 'error'" class="grow" (click)="gotoHome()">{{ 'nav.goApp' | translate }}</button>
      </div>
    </div>
  </div>
  `,
  styles: [`
    .activation-shell{display:flex;align-items:center;justify-content:center;height:70vh}
    .activation-box{max-width:520px;padding:28px;border-radius:12px;background:#fff;text-align:center;box-shadow:0 8px 30px rgba(0,0,0,0.06)}
    .activation-box h2{margin-bottom:14px;color:#000}
    .activation-box p{color:#333;margin-bottom:18px}
  .actions button{padding:10px 22px;border-radius:8px;border:none;background:linear-gradient(90deg,#3cb371,#2e8b57);color:#fff;font-weight:700;cursor:pointer;transition:transform .14s ease, box-shadow .14s ease}
  .actions button.grow:hover{transform:scale(1.06);box-shadow:0 10px 30px rgba(0,0,0,0.12)}
  .actions button.secondary{background:transparent;color:var(--primary-green-dark);border:1.5px solid var(--primary-green-dark);}
  .actions button.secondary:hover{transform:scale(1.06);box-shadow:0 10px 30px rgba(0,0,0,0.06)}
  `]
})
export class ActivationComponent implements OnInit {
  status: 'pending'|'success'|'error' = 'pending';
  message?: string | null = null;

  constructor(private route: ActivatedRoute, private auth: AuthService, private router: Router, private t: TranslationService) {}

  ngOnInit(): void {
    let token: string | null = null;

    token = this.route.snapshot.paramMap.get('token');

    if (!token) {
      try {
        const urlSegments = this.route.snapshot.url || [];
        if (urlSegments.length) {
          token = urlSegments.map(s => s.path).join('/');
        }
      } catch {
        token = null;
      }
    }

    if (!token) token = this.route.snapshot.queryParamMap.get('token');

    if (!token) {
      try {
        const href = window.location.href || '';
        const marker = '/activate-account/';
        const parts = href.split(marker);
        if (parts.length > 1) {
          let raw = parts.slice(1).join(marker);
          raw = raw.split('?')[0].split('#')[0];
          token = decodeURIComponent(raw);
        }
      } catch {
        token = null;
      }
    }

    if (!token) {
      this.status = 'error';
      this.message = 'Missing token';
      return;
    }

    this.auth.activate(token).subscribe({
      next: () => {
        this.status = 'success';
      },
      error: (err:any) => {
        this.status = 'error';
        this.message = err?.error?.errorMessage;
      }
    });
  }

  gotoLogin(){
    try { this.router.navigate(['/auth']); } catch { location.href = '/auth'; }
  }

  gotoHome(){
    try { this.router.navigate(['/']); } catch { location.href = '/'; }
  }
}
