import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { PaginatedResult } from '@app/_models/result';
import { MyPickupDto } from '@app/_models/pickups';

export interface MyPickupsFilters {
  garbageGroupId: string | null;
  statuses: number[] | null;
}

@Injectable({ providedIn: 'root' })
export class MyPickupsService {
  private readonly api = `${environment.apiUrl}/garbage-orders/my`;

  constructor(private readonly http: HttpClient) {}

  getMyPickups(
    pageNumber: number,
    pageSize: number,
    filters: MyPickupsFilters
  ): Observable<PaginatedResult<MyPickupDto[]>> {
    const params = new HttpParams()
      .set('pageNumber', String(pageNumber))
      .set('pageSize', String(pageSize));

    return this.http.post<PaginatedResult<MyPickupDto[]>>(this.api, filters, { params });
  }
}
