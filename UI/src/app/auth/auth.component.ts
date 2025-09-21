import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { CurrentUserService } from '../services/current-user.service';
import { TranslatePipe } from '../pipes/translate.pipe';
import { Router, RouterModule } from '@angular/router';
import { TranslationService } from '../services/translation.service';
import { Subscription } from 'rxjs';
import { AuthService } from '../services/auth.service';


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
  private loadingTimer: ReturnType<typeof setTimeout> | null = null;
  showLoadingText = false;
  isRegisterLoading = false;
  private registerLoadingTimer: ReturnType<typeof setTimeout> | null = null;
  showRegisterLoadingText = false;
  loginForm: FormGroup;
  registerForm: FormGroup;
  private langSub: Subscription | null = null;
  error: string | null = null;
  showActivationSection = false;


  constructor(private fb: FormBuilder, private authService: AuthService, private translation: TranslationService, private currentUser: CurrentUserService, private router: Router) {
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
    if (this.loadingTimer) clearTimeout(this.loadingTimer);
    if (this.registerLoadingTimer) clearTimeout(this.registerLoadingTimer);
    if (this.langSub) { this.langSub.unsubscribe(); this.langSub = null; }
  }

  toggleMode(event: Event) {
    event.preventDefault();
    this.isLoginMode = !this.isLoginMode;
  }

  onLogin() {
    if (!this.loginForm.valid) return;
    this.error = null;
    this.isLoading = true;
    const start = Date.now();
    // clear previous timer if any
    if (this.loadingTimer) {
      clearTimeout(this.loadingTimer);
      this.loadingTimer = null;
    }

    this.authService.login(this.loginForm.value).subscribe({
      next: (res) => {
        this.error = null;

        this.showLoadingText = true;

        const token = res?.resultModel?.token;
        if (token) {
          localStorage.setItem('authToken', token);
          const payload = this.parseJwt(token);
          const nicknameFromToken = payload?.unique_name;
          const roleClaim = payload?.role ?? res?.resultModel?.role;
          const mappedRole = this.mapRoleClaim(roleClaim);
          const nickname = nicknameFromToken || res?.resultModel?.nickname || this.loginForm.get('username')?.value;
          this.currentUser.setUser({ nickname, role: mappedRole });
        } else {

          const nickname = res?.resultModel?.nickname || this.loginForm.get('username')?.value;
          const role = this.mapRoleClaim(res?.resultModel?.role) || 'User';
          this.currentUser.setUser({ nickname, role });
        }
        const elapsed = Date.now() - start;
        const wait = Math.max(0, 1000 - elapsed);
        this.loadingTimer = setTimeout(() => {
          this.isLoading = false;
          this.showLoadingText = false;
          this.loadingTimer = null;

          try { this.router.navigate(['/portal']); } catch { location.href = '/portal'; }
        }, wait);
      },
      error: (err) => {
        this.error = this.formatErrorResponse(err);

        this.showLoadingText = false;
        if (this.loadingTimer) {
          clearTimeout(this.loadingTimer);
          this.loadingTimer = null;
        }
        this.isLoading = false;
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
    if (this.registerLoadingTimer) {
      clearTimeout(this.registerLoadingTimer);
      this.registerLoadingTimer = null;
    }

    this.authService.register({ username, email, password, role, languagePreference }).subscribe({
      next: (res) => {
        this.error = null;

        this.showRegisterLoadingText = true;
        this.showActivationSection = true;

        this.showActivationSection = true;
        const elapsed = Date.now() - start;
        const wait = Math.max(0, 1000 - elapsed);
        this.registerLoadingTimer = setTimeout(() => {
          this.isRegisterLoading = false;
          this.showRegisterLoadingText = false;
          this.registerLoadingTimer = null;

        }, wait);
      },
      error: (err) => {
        this.error = this.formatErrorResponse(err);
        this.showActivationSection = false;
        this.showRegisterLoadingText = false;
        if (this.registerLoadingTimer) {
          clearTimeout(this.registerLoadingTimer);
          this.registerLoadingTimer = null;
        }
        this.isRegisterLoading = false;
      }
    });
  }

  returnToLogin() {
    this.showActivationSection = false;
    this.isLoginMode = true;
    this.loginForm.reset();
    this.error = null;
  }


  private formatErrorResponse(err: any): string {

    if (!err) return this.translation.translate('auth.errors.unknown');
    const payload = err.error || {};

    if (payload.localizedMessage && typeof payload.localizedMessage === 'string') return payload.localizedMessage;


    if (payload.errorCode && typeof payload.errorCode === 'string') {
      if (payload.errorCode === 'ERR_TOO_SHORT') return this.translation.translate('auth.errors.password.tooShort');
      const key = `auth.errors.${payload.errorCode}`;
      const translated = this.translation.translate(key);
      if (translated !== key) return translated;
    }

    if (payload.Password && Array.isArray(payload.Password)) {
      if (payload.Password.includes('ERR_TOO_SHORT')) {
        return this.translation.translate('auth.errors.password.tooShort');
      }
    }

    if (payload.errorMessage) return payload.errorMessage;
    if (payload.message) return payload.message;
    if (typeof err.error === 'string') return err.error;
    return this.translation.translate('auth.errors.unknown');
  }


  private parseJwt(token: string): any | null {
    try {
      const parts = token.split('.');
      if (parts.length < 2) return null;
      const payload = parts[1];
      const decoded = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
      return JSON.parse(decodeURIComponent(escape(decoded)));
    } catch {
      return null;
    }
  }

  private mapRoleClaim(roleClaim: any): 'User' | 'GarbageAdmin' | string {
    if (roleClaim === undefined || roleClaim === null) return 'User';
    const rc = String(roleClaim).trim();
    if (rc === '2' || rc.toLowerCase() === 'garbageadmin') return 'GarbageAdmin';
    if (rc === '1' || rc.toLowerCase() === 'user') return 'User';
    return rc;
  }

  private mapLangForControl(lang: string | undefined): 'English' | 'Polish' {
    if (!lang) return 'Polish';
    const code = String(lang).toLowerCase();
    if (code.startsWith('pl')) return 'Polish';
    return 'English';
  }
}
