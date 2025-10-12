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
  templateUrl: './activation.component.html',
  styleUrls: ['./activation.component.css']
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
