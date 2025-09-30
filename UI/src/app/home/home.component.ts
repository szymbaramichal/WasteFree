import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { TranslatePipe } from '../pipes/translate.pipe';
import { CommonModule } from '@angular/common';
import { LoaderService } from '../services/loader.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, TranslatePipe],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent {
  constructor(private router: Router, private loader: LoaderService) {}

  goToApp() {
    try {
      this.loader.show(500);
      this.router.navigate(['/portal']).finally(() => this.loader.hide());
    } catch {
      this.loader.show(500);
      location.href = '/portal';
    }
  }
}
