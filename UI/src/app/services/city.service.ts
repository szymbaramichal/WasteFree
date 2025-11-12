import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { finalize, map, shareReplay, switchMap } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { Result } from '../_models/result';

@Injectable({
  providedIn: 'root'
})
export class CityService {
  private apiUrl = `${environment.apiUrl}/cities`;
  private http = inject(HttpClient);

  private _cities = signal<string[] | null>(null);
  cities = this._cities.asReadonly();
  private readonly maxFetchAttempts = 3;
  private inFlightRequest: Observable<string[]> | null = null;

  getCitiesList(forceRefresh = false): Observable<string[]> {
    if (forceRefresh) {
      this._cities.set(null);
      this.inFlightRequest = null;
    }

    const cached = this._cities();
    const hasCachedItems = Array.isArray(cached) && cached.length > 0;
    if (!forceRefresh && hasCachedItems) {
      return of(cached);
    }

    if (!this.inFlightRequest) {
      this.inFlightRequest = this.loadCities().pipe(
        shareReplay(1),
        finalize(() => {
          this.inFlightRequest = null;
        })
      );
    }

    return this.inFlightRequest;
  }

  private loadCities(attempt = 1): Observable<string[]> {
    return this.http.get<Result<string[]>>(`${this.apiUrl}`).pipe(
      map(response => response.resultModel ?? []),
      switchMap((cities) => {
        const shouldRetry = !cities.length && attempt < this.maxFetchAttempts;
        if (shouldRetry) {
          return this.loadCities(attempt + 1);
        }

        this._cities.set(cities.length ? cities : []);
        return of(cities);
      })
    );
  }
}
