import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '../pipes/translate.pipe';
import { CommonModule } from '@angular/common';
import { LanguageSwitcherComponent } from '../language-switcher/language-switcher.component';
import { CurrentUserService } from '../services/current-user.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink, TranslatePipe, LanguageSwitcherComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent {
  constructor(private router: Router, private currentUser: CurrentUserService) {}

  goToApp() {
    // Always navigate to portal; route guard will redirect to /auth if necessary
    try {
      this.router.navigate(['/portal']);
    } catch {
      location.href = '/portal';
    }
  }
}
