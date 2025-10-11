import { Component, EffectRef, OnDestroy, effect, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CurrentUserService } from '../services/current-user.service';
import { AsyncPipe } from '@angular/common';
import { Router, RouterModule, NavigationEnd, ActivatedRoute } from '@angular/router';
import { LanguageSwitcherComponent } from '../language-switcher/language-switcher.component';
import { TranslatePipe } from '../pipes/translate.pipe';
import { WalletService } from '../services/wallet.service';
import { filter } from 'rxjs/operators';
import { UserRole } from '../_models/user';
import { InboxService } from '../services/inbox.service';

@Component({
  selector: 'app-topbar',
  standalone: true,
  imports: [CommonModule, AsyncPipe, RouterModule, LanguageSwitcherComponent, TranslatePipe],
  templateUrl: './topbar.component.html',
  styleUrls: ['./topbar.component.css']
})
export class TopbarComponent implements OnDestroy {
  userRole = UserRole;
  currentUser = inject(CurrentUserService);
  inbox = inject(InboxService);
  visible = false;
  walletBalance$ = this.wallet.balance$;
  animateInbox = false;
  navOpen = false; // mobile navbar state
  roleKeyMap: Record<UserRole, string> = {
    [UserRole.User]: 'auth.role.user',
    [UserRole.GarbageAdmin]: 'auth.role.garbageAdmin',
    [UserRole.Admin]: 'auth.role.admin'
  };

  private inboxAnimationTimer: ReturnType<typeof setTimeout> | null = null;
  private inboxPulseEffect: EffectRef;
  private lastAnimationMark = 0;

  constructor(private router: Router, private activated: ActivatedRoute, private wallet: WalletService) {
    const check = () => {
      let route: ActivatedRoute | null = this.activated.root;
      let show = false;
      while (route) {
        if (route.snapshot && route.snapshot.data && route.snapshot.data['showTopbar']) {
          show = true;
        }
        route = route.firstChild || null;
      }
      this.visible = show;
    };

    check();
    this.router.events.pipe(filter(e => e instanceof NavigationEnd)).subscribe(() => check());

    this.inboxPulseEffect = effect(() => {
      const stamp = this.inbox.lastPush();
      if (!stamp || stamp === this.lastAnimationMark) {
        return;
      }

      this.lastAnimationMark = stamp;
      this.triggerInboxAnimation();
    });
  }

  ngOnDestroy(): void {
    if (this.inboxAnimationTimer) {
      clearTimeout(this.inboxAnimationTimer);
      this.inboxAnimationTimer = null;
    }
    this.inboxPulseEffect.destroy();
  }

  logout() {
    localStorage.removeItem('authToken');
    this.currentUser.setUser(null);
    this.router.navigate(['/']);
  }

  openInbox() {
    this.animateInbox = false;
    this.router.navigate(['/portal/inbox']);
  }

  private triggerInboxAnimation() {
    if (this.inboxAnimationTimer) {
      clearTimeout(this.inboxAnimationTimer);
    }

    this.animateInbox = true;
    this.inboxAnimationTimer = setTimeout(() => {
      this.animateInbox = false;
      this.inboxAnimationTimer = null;
    }, 2000);
  }
}
