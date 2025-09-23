import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Result } from '../_models/result';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = `${environment.apiUrl}/auth`;

  constructor(private http: HttpClient) {}

  register(data: any): Observable<Result<User>> {
    return this.http.post<Result<User>>(`${this.apiUrl}/register`, data);
  }

  login(data: any): Observable<Result<User>> {
    return this.http.post<Result<User>>(`${this.apiUrl}/login`, data);
  }

  activate(token: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/activate-account?token=${encodeURIComponent(token)}`, {});
  }
}

