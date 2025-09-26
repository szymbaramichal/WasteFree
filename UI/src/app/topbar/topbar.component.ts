import { Component, inject } from '@angular/core';
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
export class TopbarComponent {
  userRole = UserRole;
  currentUser = inject(CurrentUserService);
  inbox = inject(InboxService);
  visible = false;
  walletBalance$ = this.wallet.balance$;
  animateInbox = false;
  roleKeyMap: Record<UserRole, string> = {
    [UserRole.User]: 'auth.role.user',
    [UserRole.GarbageAdmin]: 'auth.role.garbageAdmin',
    [UserRole.Admin]: 'auth.role.admin'
  };

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
  }

  logout() {
    localStorage.removeItem('authToken');
    this.currentUser.setUser(null);
    this.router.navigate(['/']);
  }

  openInbox() {
    this.animateInbox = false;
    this.inbox.refresh();
    this.router.navigate(['/portal/inbox']);
  }
}
