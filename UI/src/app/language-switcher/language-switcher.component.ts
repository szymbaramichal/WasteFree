import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslationService } from '../services/translation.service';

@Component({
  selector: 'app-language-switcher',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './language-switcher.component.html',
  styleUrls: ['./language-switcher.component.css']
})
export class LanguageSwitcherComponent {
  dropdownOpen = false;

  constructor(public t: TranslationService) {}

  toggle(event?: Event) {
    if (event) event.stopPropagation();
    this.dropdownOpen = !this.dropdownOpen;
  }

  select(lang: string) {
    this.t.setLanguage(lang);
    this.dropdownOpen = false;
  }

  close() {
    this.dropdownOpen = false;
  }
}
