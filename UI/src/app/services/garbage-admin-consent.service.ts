import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Result } from '../_models/result';

@Injectable({
  providedIn: 'root'
})
export class GarbageAdminConsentService {
  private readonly http = inject(HttpClient);
  private readonly consentUrl = `${environment.apiUrl}/garbage-admin-consents`;
  private readonly acceptUrl = `${environment.apiUrl}/user/consents/accept`;

  getConsent(): Observable<Result<string>> {
    return this.http.get<Result<string>>(this.consentUrl);
  }

  acceptConsent(): Observable<Result<unknown>> {
    return this.http.patch<Result<unknown>>(this.acceptUrl, {});
  }
}
