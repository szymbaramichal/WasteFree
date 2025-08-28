import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from './auth.service';
import { TranslatePipe } from '../pipes/translate.pipe';

@Component({
  selector: 'app-auth',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslatePipe],
  templateUrl: './auth.component.html',
  styleUrls: ['./auth.component.css']
})
export class AuthComponent {
  private bodyClass = 'auth-bg';
  isLoginMode = true;
  loginForm: FormGroup;
  registerForm: FormGroup;
  error: string | null = null;
  showActivationSection = false;

  constructor(private fb: FormBuilder, private authService: AuthService) {
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
  }

  toggleMode(event: Event) {
    event.preventDefault();
    this.isLoginMode = !this.isLoginMode;
  }

  onLogin() {
    if (!this.loginForm.valid) return;
    this.error = null;
    this.authService.login(this.loginForm.value).subscribe({
      next: (res) => {
        console.log('Zalogowano!', res);
        this.error = null;
        // saving token to localStorage
        const token = res?.resultModel?.token;
        if (token) {
          localStorage.setItem('authToken', token);
        }
        // TODO: navigate
      },
      error: (err) => {
        console.error('Błąd logowania', err);
        this.error = this.formatErrorResponse(err);
      }
    });
  }

  onRegister() {
    if (!this.registerForm.valid) return;
    this.error = null;
    this.showActivationSection = false;
    const { username, email, password, role } = this.registerForm.value;
    this.authService.register({ username, email, password, role }).subscribe({
      next: (res) => {
        console.log('Zarejestrowano!', res);
        this.error = null;
        this.showActivationSection = true;
      },
      error: (err) => {
        console.error('Błąd rejestracji', err);
        this.error = this.formatErrorResponse(err);
        this.showActivationSection = false;
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
