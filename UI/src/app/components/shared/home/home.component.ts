import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { TranslatePipe } from '@app/pipes/translate.pipe';
import { CommonModule } from '@angular/common';
import { LoaderService } from '@app/services/loader.service';
import { CityService } from '@app/services/city.service';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, TranslatePipe],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit {
  cities: string[] = [];

  constructor(private router: Router, private loader: LoaderService, private cityService: CityService) {}

  async ngOnInit(): Promise<void> {
    this.cities = await firstValueFrom(this.cityService.getCitiesList());
  }

  goToApp() {
    try {
      this.loader.show(500);
      this.router.navigate(['/portal']).finally(() => this.loader.hide());
    } catch {
      this.loader.show(500);
      location.href = '/portal';
    }
  }

  trackCity(_: number, city: string) {
    return city;
  }
}
