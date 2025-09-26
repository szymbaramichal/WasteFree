import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { CurrentUserService } from '../services/current-user.service';
import { TranslatePipe } from '../pipes/translate.pipe';
import { Router, RouterModule } from '@angular/router';
import { TranslationService } from '../services/translation.service';
import { Subscription } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { User, UserRole } from '../_models/user';
import { InboxService } from '../services/inbox.service';

@Component({
  selector: 'app-auth',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslatePipe, RouterModule],
  templateUrl: './auth.component.html',
  styleUrls: ['./auth.component.css']
})
export class AuthComponent {
  private bodyClass = 'auth-bg';

  isLoginMode = true;

  isLoading = false;
  showLoadingText = false;

  isRegisterLoading = false;
  showRegisterLoadingText = false;
  showActivationSection = false;

  loginForm: FormGroup;
  registerForm: FormGroup;

  private langSub: Subscription | null = null;

  error: string | null = null;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private translation: TranslationService,
    private currentUser: CurrentUserService,
    private router: Router, 
    private inbox: InboxService
  ) {
    this.loginForm = this.fb.group({
      username: ['', Validators.required],
      password: ['', Validators.required]
    });

    this.registerForm = this.fb.group({
      username: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required],
      role: ['User', Validators.required],
      languagePreference: ['English', Validators.required]
    });

    const initialLang = this.mapLangForControl(this.translation.currentLang);
    this.registerForm.get('languagePreference')?.setValue(initialLang);

    this.langSub = this.translation.onLangChange.subscribe((l) => {
      const mapped = this.mapLangForControl(l);
      this.registerForm.get('languagePreference')?.setValue(mapped);
    });
  }

  ngOnInit(): void {
    document.body.classList.add(this.bodyClass);
  }

  ngOnDestroy(): void {
    document.body.classList.remove(this.bodyClass);
    if (this.langSub) { this.langSub.unsubscribe(); this.langSub = null; }
  }

  toggleMode(event: Event) {
    event.preventDefault();
    this.isLoginMode = !this.isLoginMode;
    this.error = null;
  }

  onLogin() {
    if (!this.loginForm.valid) return;

    this.error = null;
    this.isLoading = true;

    const start = Date.now();
    let success = false;

    this.authService.login(this.loginForm.value).subscribe({
      next: (res) => {
        this.error = null;
        this.showLoadingText = true;

        this.applyAuthResult(res.resultModel);

        this.inbox.refresh();

        success = true;
        this.finishLoading(start, success, () => {
          this.isLoading = false;
          this.showLoadingText = false;
          try { this.router.navigate(['/portal']); } catch { location.href = '/portal'; }
        });
      },
      error: (err) => {
        this.error = this.extractApiError(err);
        success = false;
        this.finishLoading(start, success, () => {
          this.isLoading = false;
          this.showLoadingText = false;
        });
      }
    });
  }

  onRegister() {
    if (!this.registerForm.valid) return;

    this.error = null;
    this.showActivationSection = false;

    const { username, email, password, role, languagePreference } = this.registerForm.value;

    this.isRegisterLoading = true;
    const start = Date.now();
    let success = false;

    this.authService.register({ username, email, password, role, languagePreference }).subscribe({
      next: () => {
        this.error = null;
        this.showRegisterLoadingText = true;
        this.showActivationSection = true;

        success = true;
        this.finishLoading(start, success, () => {
          this.isRegisterLoading = false;
          this.showRegisterLoadingText = false;
        });
      },
      error: (err) => {
        this.error = this.extractApiError(err);
        this.showActivationSection = false;
        this.showRegisterLoadingText = false;

        success = false;
        this.finishLoading(start, success, () => {
          this.isRegisterLoading = false;
          this.showRegisterLoadingText = false;
        });
      }
    });
  }

  returnToLogin() {
    this.showActivationSection = false;
    this.isLoginMode = true;
    this.loginForm.reset();
    this.error = null;
  }

  // Helpers

  // Ensures minimum spinner time on success; immediate stop on error
  private finishLoading(start: number, isSuccess: boolean, done: () => void, minMs = 1000) {
    const elapsed = Date.now() - start;
    const wait = isSuccess ? Math.max(0, minMs - elapsed) : 0;
    setTimeout(done, wait);
  }

  private applyAuthResult(res: User) {
    const token = res.token;

    if (token) {
      localStorage.setItem('authToken', token);
      this.currentUser.setUser({
        id: res.id,
        username: res.username,
        role: this.praseRole(res.userRole)
      });
      return;
    }
  }

  private mapLangForControl(lang: string | undefined): 'English' | 'Polish' {
    if (!lang) return 'Polish';
    const code = String(lang).toLowerCase();
    if (code.startsWith('pl')) return 'Polish';
    return 'English';
  }

  private praseRole(val: string): UserRole {
  if (Object.values(UserRole).includes(val as UserRole)) {
    return val as UserRole;
  }

  return UserRole.User;
  }

  // 400 and 422
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
    } catch {
      return '';
    }
  }
}
