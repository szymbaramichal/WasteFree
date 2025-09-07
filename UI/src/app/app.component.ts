import { Component } from '@angular/core';
import { RouterOutlet, Router, NavigationEnd } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TranslatePipe } from './pipes/translate.pipe';
import { TranslationService } from './services/translation.service';
import { LanguageSwitcherComponent } from './language-switcher/language-switcher.component';
import { TopbarComponent } from './topbar/topbar.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, TranslatePipe, LanguageSwitcherComponent, TopbarComponent],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'WasteFree';
  dropdownOpen = false;
  showGlobalLang = true;

  constructor(public t: TranslationService, private router: Router) {
    // hide global language switcher on portal route to avoid duplicate controls
    const check = () => {
      this.showGlobalLang = this.router.url !== '/portal';
    };
    check();
    this.router.events.subscribe(e => { if (e instanceof NavigationEnd) check(); });
  }

  toggleDropdown() {
    this.dropdownOpen = !this.dropdownOpen;
  }

  closeDropdown() {
    this.dropdownOpen = false;
  }

  selectLanguage(lang: string) {
    this.t.setLanguage(lang);
    this.closeDropdown();
  }
}
