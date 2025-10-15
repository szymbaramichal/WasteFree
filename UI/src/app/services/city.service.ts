import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Result } from '../_models/result';

@Injectable({
  providedIn: 'root'
})
export class CityService {
  private apiUrl = `${environment.apiUrl}/cities`;
  private http = inject(HttpClient);
  
  getCitiesList() : Observable<Result<string[]>> {
    return this.http.get<Result<string[]>>(`${this.apiUrl}`);
  }
}
