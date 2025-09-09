import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CurrentUserService } from '../services/current-user.service';
import { AsyncPipe } from '@angular/common';
import { Router, RouterModule, NavigationEnd, ActivatedRoute } from '@angular/router';
import { LanguageSwitcherComponent } from '../language-switcher/language-switcher.component';
import { WalletService } from '../services/wallet.service';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-topbar',
  standalone: true,
  imports: [CommonModule, AsyncPipe, RouterModule, LanguageSwitcherComponent],
  templateUrl: './topbar.component.html',
  styleUrls: ['./topbar.component.css']
})
export class TopbarComponent {
  user$ = this.currentUser.user$;
  visible = false;
  walletBalance$ = this.wallet.balance$;

  constructor(private currentUser: CurrentUserService, private router: Router, private activated: ActivatedRoute, private wallet: WalletService) {
    // initialize and react to navigation changes
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

    // check on construction
    check();
    // update on navigation end
    this.router.events.pipe(filter(e => e instanceof NavigationEnd)).subscribe(() => check());
  }

  logout() {
    localStorage.removeItem('authToken');
    this.currentUser.setUser(null);
    this.router.navigate(['/']);
  }
}
