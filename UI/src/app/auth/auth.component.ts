import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from './auth.service';

@Component({
  selector: 'app-auth',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './auth.component.html',
  styleUrls: ['./auth.component.css']
})
export class AuthComponent {
  private bodyClass = 'auth-bg';
  isLoginMode = true;
  loginForm: FormGroup;
  registerForm: FormGroup;
  error: string | null = null;

  constructor(private fb: FormBuilder, private authService: AuthService) {
    this.loginForm = this.fb.group({
      username: ['', Validators.required],
      password: ['', Validators.required]
    });

    this.registerForm = this.fb.group({
      username: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
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
        // TODO: navigate / store token
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
    this.authService.register(this.registerForm.value).subscribe({
      next: (res) => {
        console.log('Zarejestrowano!', res);
        this.error = null;
      },
      error: (err) => {
        console.error('Błąd rejestracji', err);
        this.error = this.formatErrorResponse(err);
      }
    });
  }

  private formatErrorResponse(err: any): string {
    if (!err) return 'Nieznany błąd';

    // Helper to translate specific backend error messages to Polish
    function translateToPolish(msg: string): string {
      if (!msg) return msg;
      if (msg === 'Login or password is incorrect') return 'Nieprawidłowy login lub hasło';
      if (msg === "Hasło: 'Password' is required and must be at least 8 characters long.") return "Hasło jest wymagane i musi mieć co najmniej 8 znaków.";
      if (msg === 'Username is already taken') return 'Nazwa użytkownika jest już zajęta';
      // Add more translations as needed
      return msg;
    }

    // Handle 422 Unprocessable Entity (validation errors)
    if (err.status === 422) {
      const payload = err.error || {};
      const errorsObj = payload.errors || payload.Errors || payload.validationErrors || payload?.modelState || (typeof payload === 'object' ? payload : null);
      if (errorsObj && typeof errorsObj === 'object') {
        const messages: string[] = [];
        const fieldNames: Record<string, string> = {
          Password: 'Hasło',
          password: 'Hasło',
          Email: 'Email',
          email: 'Email',
          Username: 'Nazwa użytkownika',
          username: 'Nazwa użytkownika'
        };
        for (const key of Object.keys(errorsObj)) {
          const val = errorsObj[key];
          if (Array.isArray(val)) {
            messages.push(...val.map((v: any) => {
              const field = fieldNames[key] || key;
              if (typeof v === 'string') {
                return translateToPolish(`${field}: ${v}`);
              }
              return `${field}: ${JSON.stringify(v)}`;
            }));
          } else if (typeof val === 'string') {
            const field = fieldNames[key] || key;
            messages.push(translateToPolish(`${field}: ${val}`));
          } else if (typeof val === 'object') {
            const field = fieldNames[key] || key;
            messages.push(`${field}: ${JSON.stringify(val)}`);
          }
        }
        if (messages.length) return messages.join('; ');
      }
      // fallback to errorMessage or message
      if (payload.errorMessage) return translateToPolish(payload.errorMessage);
      if (payload.message) return translateToPolish(payload.message);
      return 'Wystąpiły błędy walidacji';
    }

    // Generic handling for other statuses
    let msg = err?.error?.errorMessage || err?.error?.message || (err?.message ? String(err.message) : 'Błąd sieci');
    return translateToPolish(msg);
  }
}
