import { Component, inject } from '@angular/core';
import { RouterOutlet, Router, NavigationEnd } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TranslationService } from '@app/services/translation.service';
import { LanguageSwitcherComponent } from '@components/shared/language-switcher/language-switcher.component';
import { TopbarComponent } from '@components/shared/topbar/topbar.component';
import { LoaderOverlayComponent } from '@components/shared/loader-overlay/loader-overlay.component';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, LanguageSwitcherComponent, TopbarComponent, LoaderOverlayComponent],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'WasteFree';
  dropdownOpen = false;
  showGlobalLang = true;
  toastr = inject(ToastrService);

  constructor(public t: TranslationService, private router: Router) {
    const check = () => {
      const url = this.router.url || '';
      this.showGlobalLang = !url.startsWith('/portal');
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
