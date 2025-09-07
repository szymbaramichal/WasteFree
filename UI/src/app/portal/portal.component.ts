import { Component, OnInit, OnDestroy } from '@angular/core';
import { CurrentUserService } from '../services/current-user.service';
import { CommonModule } from '@angular/common';
import { LanguageSwitcherComponent } from '../language-switcher/language-switcher.component';

@Component({
  selector: 'app-portal',
  standalone: true,
  imports: [
  CommonModule,
  LanguageSwitcherComponent
  ],
  templateUrl: './portal.component.html',
  styleUrls: ['./portal.component.css']
})
export class PortalComponent {
  private bodyClass = 'portal-bg';

  constructor(public currentUser: CurrentUserService) {}

  ngOnInit(): void {
    document.body.classList.add(this.bodyClass);
  }

  ngOnDestroy(): void {
    document.body.classList.remove(this.bodyClass);
  }

}
