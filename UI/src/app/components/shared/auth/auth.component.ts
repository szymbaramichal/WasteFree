import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { CurrentUserService } from '@app/services/current-user.service';
import { TranslatePipe } from '@app/pipes/translate.pipe';
import { Router, RouterModule } from '@angular/router';
import { TranslationService } from '@app/services/translation.service';
import { Subscription } from 'rxjs';
import { AuthService } from '@app/services/auth.service';
import { CityService } from '@app/services/city.service';
import { RegisterRequest } from '@app/_models/auth';
import { User } from '@app/_models/user';
import { buildAddressFormGroup } from '@app/forms/address-form';
import { SignalRService } from '@app/services/signalr.service';
import { ProfileService } from '@app/services/profile.service';
import { WalletService } from '@app/services/wallet.service';

@Component({
  selector: 'app-auth',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslatePipe, RouterModule],
  templateUrl: './auth.component.html',
  styleUrls: ['./auth.component.css']
})
export class AuthComponent {
  private bodyClass = 'auth-bg';
  private signalR = inject(SignalRService);

  isLoginMode = true;

  isLoading = false;
  showLoadingText = false;

  isRegisterLoading = false;
  showRegisterLoadingText = false;
  showActivationSection = false;
  registerStep = 1;

  private readonly totalRegisterSteps = 2;
  private readonly registerStepControlPaths: string[][] = [
    ['username', 'email', 'password', 'role'],
    ['languagePreference', 'address.city', 'address.postalCode', 'address.street']
  ];

  loginForm: FormGroup;
  registerForm: FormGroup;

  cities: string[] = [];
  isLoadingCities = false;
  cityLoadError = false;

  private langSub: Subscription | null = null;
  private citySub: Subscription | null = null;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private translation: TranslationService,
    private currentUser: CurrentUserService,
    private profileService: ProfileService,
    private wallet: WalletService,
    private router: Router,
    private cityService: CityService,
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
      languagePreference: ['English', Validators.required],
      address: buildAddressFormGroup(this.fb)
    });

    const initialLang = this.mapLangForControl(this.translation.currentLang);
    this.registerForm.patchValue({ languagePreference: initialLang });

    this.langSub = this.translation.onLangChange.subscribe((l) => {
      const mapped = this.mapLangForControl(l);
      this.registerForm.get('languagePreference')?.setValue(mapped);
    });

    this.cities = cityService.cities() ?? [];
    if (this.cities.length) {
      this.applyDefaultCityFromList();
    }
  }

  ngOnInit(): void {
    document.body.classList.add(this.bodyClass);
    this.loadCities();
  }

  ngOnDestroy(): void {
    document.body.classList.remove(this.bodyClass);
    if (this.langSub) { this.langSub.unsubscribe(); this.langSub = null; }
    if (this.citySub) { this.citySub.unsubscribe(); this.citySub = null; }
  }

  toggleMode(event: Event) {
    event.preventDefault();
    this.isLoginMode = !this.isLoginMode;
    this.showActivationSection = false;
    this.registerStep = 1;
    if (!this.isLoginMode) {
      this.loadCities();
    }
  }

  onLogin() {
    if (!this.loginForm.valid) return;

    this.isLoading = true;

    const start = Date.now();
    let success = false;

    this.authService.login(this.loginForm.value).subscribe({
      next: (res) => {
        this.showLoadingText = true;

        const applied = this.applyAuthResult(res.resultModel);
        success = applied;

        if (!applied) {
          this.finishLoading(start, success, () => {
            this.isLoading = false;
            this.showLoadingText = false;
          });
          return;
        }

        this.finishLoading(start, success, () => {
          this.isLoading = false;
          this.showLoadingText = false;
          this.signalR.startConnection();
          try { this.router.navigate(['/portal']); } catch { location.href = '/portal'; }
        });
      },
      error: () => {
        this.finishLoading(start, success, () => {
          this.isLoading = false;
          this.showLoadingText = false;
        });
      }
    });
  }

  onRegister() {
    this.showActivationSection = false;

    if (!this.isRegisterLastStep) {
      this.goToNextRegisterStep();
      return;
    }

    this.markCurrentStepControlsAsTouched();

    if (!this.registerForm.valid) {
      this.registerForm.updateValueAndValidity({ onlySelf: false, emitEvent: true });
      return;
    }

    const { username, email, password, role, languagePreference, address } = this.registerForm.value as RegisterRequest;

    this.isRegisterLoading = true;
    const start = Date.now();
    let success = false;

    this.authService.register({ username, email, password, role, languagePreference, address }).subscribe({
      next: () => {
        this.showRegisterLoadingText = true;
        this.showActivationSection = true;
        success = true;

        this.finishLoading(start, success, () => {
          this.isRegisterLoading = false;
          this.showRegisterLoadingText = false;
          this.resetRegisterFormDefaults();
        });
      },
      error: () => {
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
    this.resetRegisterFormDefaults();
  }

  get isRegisterFirstStep(): boolean {
    return this.registerStep === 1;
  }

  get isRegisterLastStep(): boolean {
    return this.registerStep === this.totalRegisterSteps;
  }

  get isCurrentRegisterStepValid(): boolean {
    return this.getControlPathsForCurrentStep().every((path) => {
      const control = this.registerForm.get(path);
      return control ? control.valid : false;
    });
  }

  private goToNextRegisterStep() {
    if (this.isRegisterLastStep) return;
    this.markCurrentStepControlsAsTouched();
    if (!this.isCurrentRegisterStepValid) {
      this.registerForm.updateValueAndValidity({ onlySelf: false, emitEvent: true });
      return;
    }
    this.registerStep = Math.min(this.registerStep + 1, this.totalRegisterSteps);
  }

  goToPreviousRegisterStep() {
    if (this.isRegisterFirstStep) return;
    this.registerStep = Math.max(1, this.registerStep - 1);
  }

  private markCurrentStepControlsAsTouched() {
    this.getControlPathsForCurrentStep().forEach((path) => {
      const control = this.registerForm.get(path);
      control?.markAsTouched();
      control?.markAsDirty();
    });
  }

  private getControlPathsForCurrentStep(): string[] {
    return this.registerStepControlPaths[this.registerStep - 1] ?? [];
  }

  private loadCities(force = false) {
    const cachedCities = this.cityService.cities();
    if (!force && Array.isArray(cachedCities) && cachedCities.length) {
      this.cities = cachedCities;
      this.applyDefaultCityFromList();
      return;
    }

    if (this.citySub) {
      this.citySub.unsubscribe();
      this.citySub = null;
    }

    this.isLoadingCities = true;
    this.cityLoadError = false;

  this.citySub = this.cityService.getCitiesList(force).subscribe({
      next: (cities) => {
        this.cities = cities;
        this.isLoadingCities = false;
        this.cityLoadError = !cities.length;
        this.applyDefaultCityFromList();
      },
      error: () => {
        this.isLoadingCities = false;
        this.cityLoadError = true;
      }
    });
  }

  private applyDefaultCityFromList() {
    if (!this.cities.length) {
      return;
    }
    const cityControl = this.registerForm.get('address.city');
    const currentValue = cityControl?.value;
    if (!cityControl) {
      return;
    }
    if (!currentValue) {
      cityControl.setValue(this.cities[0], { emitEvent: false });
      cityControl.markAsPristine();
      cityControl.markAsUntouched();
    }
  }


  private finishLoading(start: number, isSuccess: boolean, done: () => void, minMs = 1000) {
    const elapsed = Date.now() - start;
    const wait = isSuccess ? Math.max(0, minMs - elapsed) : 0;
    setTimeout(done, wait);
  }

  private applyAuthResult(res: User | null): boolean {
    if (!res) {
      return false;
    }
    const token = res.token;

    if (token) {
      localStorage.setItem('authToken', token);
      this.profileService.clear();
      this.wallet.resetState();
      void this.wallet.refreshBalance();
      this.currentUser.setUser({
        id: res.id,
        username: res.username,
        role: res.userRole,
        acceptedConsents: res.acceptedConsents,
        avatarUrl: res.avatarUrl ?? null
      });
      return true;
    }

    return false;
  }

  private mapLangForControl(lang: string | undefined): 'English' | 'Polish' {
    if (!lang) return 'Polish';
    const code = String(lang).toLowerCase();
    if (code.startsWith('pl')) return 'Polish';
    return 'English';
  }
  private resetRegisterFormDefaults() {
    const initialLang = this.mapLangForControl(this.translation.currentLang);
    const defaultCity = this.cities[0] ?? '';

    this.registerForm.reset({
      username: '',
      email: '',
      password: '',
      role: 'User',
      languagePreference: initialLang,
      address: {
        city: defaultCity,
        postalCode: '',
        street: ''
      }
    });
    this.registerStep = 1;
  }

  private get registerAddressGroup(): FormGroup {
    return this.registerForm.get('address') as FormGroup;
  }
}
