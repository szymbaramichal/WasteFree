import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from './auth.service';
import { CurrentUserService } from '../services/current-user.service';
import { TranslatePipe } from '../pipes/translate.pipe';
import { Router, RouterModule } from '@angular/router';

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
  error: string | null = null;
  showActivationSection = false;

  constructor(private fb: FormBuilder, private authService: AuthService, private currentUser: CurrentUserService, private router: Router) {
    this.loginForm = this.fb.group({
      username: ['', Validators.required],
      password: ['', Validators.required]
    });

    this.registerForm = this.fb.group({
      username: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required],
      role: ['User', Validators.required] 
    });
  }

  ngOnInit(): void {
    document.body.classList.add(this.bodyClass);
  }

  ngOnDestroy(): void {
    document.body.classList.remove(this.bodyClass);
  if (this.loadingTimer) clearTimeout(this.loadingTimer);
  if (this.registerLoadingTimer) clearTimeout(this.registerLoadingTimer);
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
        console.log('Zalogowano!', res);
        this.error = null;
        // Only show the localized loading text when login succeeded
        this.showLoadingText = true;
        // saving token to localStorage
        const token = res?.resultModel?.token;
        if (token) {
          localStorage.setItem('authToken', token);
        }
  // set current user from response if available, fallback to form username
  const nickname = res?.resultModel?.nickname || this.loginForm.get('username')?.value;
  const role = res?.resultModel?.role || 'User';
  this.currentUser.setUser({ nickname, role });
        const elapsed = Date.now() - start;
        const wait = Math.max(0, 1000 - elapsed);
        this.loadingTimer = setTimeout(() => {
          this.isLoading = false;
          this.showLoadingText = false;
          this.loadingTimer = null;
          // navigate to portal after successful login
          try { this.router.navigate(['/portal']); } catch { location.href = '/portal'; }
        }, wait);
      },
      error: (err) => {
        console.error('Błąd logowania', err);
        this.error = this.formatErrorResponse(err);
        // On error hide loading immediately (no spinner, no text)
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
  const { username, email, password, role } = this.registerForm.value;
    this.isRegisterLoading = true;
    const start = Date.now();
    if (this.registerLoadingTimer) {
      clearTimeout(this.registerLoadingTimer);
      this.registerLoadingTimer = null;
    }

    this.authService.register({ username, email, password, role }).subscribe({
      next: (res) => {
        console.log('Zarejestrowano!', res);
        this.error = null;
        // show loading text only on success
        this.showRegisterLoadingText = true;
        this.showActivationSection = true;
  // after successful register set current user
  const nickname = res?.resultModel?.nickname || username;
  const resolvedRole = res?.resultModel?.role || role || 'User';
  this.currentUser.setUser({ nickname, role: resolvedRole });
        const elapsed = Date.now() - start;
        const wait = Math.max(0, 1000 - elapsed);
        this.registerLoadingTimer = setTimeout(() => {
          this.isRegisterLoading = false;
          this.showRegisterLoadingText = false;
          this.registerLoadingTimer = null;
          // navigate to portal after successful register
          try { this.router.navigate(['/portal']); } catch { location.href = '/portal'; }
        }, wait);
      },
      error: (err) => {
        console.error('Błąd rejestracji', err);
        this.error = this.formatErrorResponse(err);
        this.showActivationSection = false;
        // hide loading immediately on error
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
    if (!err) return 'Nieznany błąd';
    const payload = err.error || {};
    if (payload.Password && Array.isArray(payload.Password)) {
      if (payload.Password.includes('ERR_TOO_SHORT')) {
        return 'Hasło jest za krótkie (minimum 8 znaków).';
      }
    }
    if (payload.errorMessage) return payload.errorMessage;
    if (payload.message) return payload.message;
    if (typeof err.error === 'string') return err.error;
    return 'Nieznany błąd';
  }
}
