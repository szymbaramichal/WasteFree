import { Component, OnInit, OnDestroy } from '@angular/core';
import { CurrentUserService } from '../services/current-user.service';
import { CommonModule } from '@angular/common';
import { LanguageSwitcherComponent } from '../language-switcher/language-switcher.component';
import { RouterModule } from '@angular/router';
import { TranslatePipe } from '../pipes/translate.pipe';

@Component({
  selector: 'app-portal',
  standalone: true,
  imports: [
  CommonModule,
  LanguageSwitcherComponent,
  RouterModule,
  TranslatePipe
  ],
  templateUrl: './portal.component.html',
  styleUrls: ['./portal.component.css']
})
export class PortalComponent {
  private bodyClass = 'portal-bg';
  sidebarCollapsed = false;

  constructor(public currentUser: CurrentUserService) {}

  ngOnInit(): void {
    document.body.classList.add(this.bodyClass);
    try { this.sidebarCollapsed = localStorage.getItem('portal.sidebar.collapsed') === '1'; } catch {}
  }

  ngOnDestroy(): void {
    document.body.classList.remove(this.bodyClass);
  }

  toggleSidebar(): void {
    this.sidebarCollapsed = !this.sidebarCollapsed;
    try { localStorage.setItem('portal.sidebar.collapsed', this.sidebarCollapsed ? '1' : '0'); } catch {}
  }

  closeSidebar(): void {
    if (!this.sidebarCollapsed) {
      this.sidebarCollapsed = true;
      try { localStorage.setItem('portal.sidebar.collapsed', '1'); } catch {}
    }
  }

  onNavClick(_: MouseEvent): void {
    // Auto-close on small screens 
    if (window.innerWidth < 992) {
      this.closeSidebar();
    }
  }

}
