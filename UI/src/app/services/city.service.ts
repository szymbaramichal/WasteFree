import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { map, tap } from 'rxjs/operators';
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

  getCitiesList(): Observable<string[]> {
    const cached = this._cities();
    if (cached !== null) {
      return of(cached);
    }

    return this.http.get<Result<string[]>>(`${this.apiUrl}`).pipe(
      map(response => response.resultModel ?? []),
      tap(cities => this._cities.set(cities))
    );
  }
}
