import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Result } from '@app/_models/result';
import { MyPickupDto } from '@app/_models/pickups';

@Injectable({ providedIn: 'root' })
export class MyPickupsService {
  private readonly api = `${environment.apiUrl}/garbage-orders/my`;

  constructor(private readonly http: HttpClient) {}

  getMyPickups(pageNumber: number, pageSize: number): Observable<Result<MyPickupDto[]>> {
    const params = new HttpParams()
      .set('pageNumber', String(pageNumber))
      .set('pageSize', String(pageSize));

    return this.http.get<Result<MyPickupDto[]>>(this.api, { params });
  }
}
