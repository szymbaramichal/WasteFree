import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { TranslatePipe } from '@app/pipes/translate.pipe';
import { CommonModule } from '@angular/common';
import { LoaderService } from '@app/services/loader.service';
import { CityService } from '@app/services/city.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, TranslatePipe],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent {
  cities: string[] = [];
  isLoadingCities = false;
  citiesError = false;
  readonly cityLoaderDots = Array.from({ length: 4 }, (_, idx) => idx);

  constructor(private router: Router, private loader: LoaderService, private city: CityService) {
    this.loadSupportedCities();
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

  private loadSupportedCities() {
    this.isLoadingCities = true;
    this.citiesError = false;

    this.city.getCitiesList().subscribe({
      next: response => {
        const rawCities = response.resultModel ?? [];
        this.cities = rawCities
          .filter(city => !!city && city.trim().length > 0)
          .map(city => city.trim());
        this.isLoadingCities = false;
      },
      error: () => {
        this.isLoadingCities = false;
        this.citiesError = true;
      }
    });
  }
}
