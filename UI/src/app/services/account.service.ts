import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Result } from '../_models/result';
import { UserStats } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/user`;

  getStats(): Observable<Result<UserStats>> {
    return this.http.get<Result<UserStats>>(`${this.apiUrl}/stats`);
  }
}
