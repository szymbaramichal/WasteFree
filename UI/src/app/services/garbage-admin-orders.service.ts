import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { PaginatedResult, Result } from '@app/_models/result';
import { GarbageAdminOrderDto } from '@app/_models/garbage-orders';

@Injectable({ providedIn: 'root' })
export class GarbageAdminOrdersService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/garbage-admin/orders`;

  getWaitingOrders(pageNumber: number, pageSize: number): Observable<PaginatedResult<GarbageAdminOrderDto[]>> {
    const params = new HttpParams()
      .set('pageNumber', String(pageNumber))
      .set('pageSize', String(pageSize));

    return this.http.get<PaginatedResult<GarbageAdminOrderDto[]>>(this.baseUrl, { params });
  }

  getCurrentOrders(pageNumber: number, pageSize: number): Observable<PaginatedResult<GarbageAdminOrderDto[]>> {
    const params = new HttpParams()
      .set('pageNumber', String(pageNumber))
      .set('pageSize', String(pageSize));

    return this.http.get<PaginatedResult<GarbageAdminOrderDto[]>>(`${this.baseUrl}/current`, { params });
  }

  acceptOrder(orderId: string): Observable<Result<GarbageAdminOrderDto>> {
    const encoded = encodeURIComponent(orderId);
    return this.http.post<Result<GarbageAdminOrderDto>>(`${this.baseUrl}/${encoded}/accept`, {});
  }
}
