import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { TranslatePipe } from '../pipes/translate.pipe';
import { CommonModule } from '@angular/common';
import { CurrentUserService } from '../services/current-user.service';
import { LoaderService } from '../services/loader.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, TranslatePipe],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent {
  constructor(private router: Router, private currentUser: CurrentUserService, private loader: LoaderService) {}

  goToApp() {
    // Always navigate to portal; route guard will redirect to /auth if necessary
    try {
      this.loader.show(500);
      this.router.navigate(['/portal']).finally(() => this.loader.hide());
    } catch {
      // Fallback for non-SPA navigation
      this.loader.show(500);
      location.href = '/portal';
    }
  }
}
