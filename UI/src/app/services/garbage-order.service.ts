import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { Observable, of } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { PaginatedResult, Pager, Result } from '@app/_models/result';
import {
  CalculateGarbageOrderRequest,
  CreateGarbageOrderRequest,
  GarbageOrderCostDto,
  GarbageOrderDto,
  GarbageOrderFilterRequest
} from '@app/_models/garbage-orders';

export const USER_ORDERS_PAGE_SIZE = 200;

@Injectable({
  providedIn: 'root'
})
export class GarbageOrderService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}`;

  private readonly ordersSignal = signal<GarbageOrderDto[]>([]);
  private readonly pagerSignal = signal<Pager | null>(null);
  private readonly loadedSignal = signal(false);

  readonly orders = this.ordersSignal.asReadonly();
  readonly pager = this.pagerSignal.asReadonly();
  readonly hasLoaded = this.loadedSignal.asReadonly();

  getMyOrders(pageNumber: number, pageSize: number): Observable<PaginatedResult<GarbageOrderDto[]>> {
    const params = new URLSearchParams({
      pageNumber: String(pageNumber),
      pageSize: String(pageSize)
    });
    return this.http
      .get<PaginatedResult<GarbageOrderDto[]>>(`${this.apiUrl}/garbage-orders/my?${params.toString()}`)
      .pipe(tap((res) => this.setCache(res)));
  }

  payForOrder(groupId: string, orderId: string): Observable<Result<GarbageOrderDto>> {
    const g = encodeURIComponent(groupId);
    const o = encodeURIComponent(orderId);
    return this.http
      .post<Result<GarbageOrderDto>>(`${this.apiUrl}/garbage-group/${g}/order/${o}/payment`, {})
      .pipe(tap((res) => {
        if (res.resultModel) {
          this.upsertOrder(res.resultModel);
        }
      }));
  }

  payAdditionalUtilizationFee(groupId: string, orderId: string): Observable<Result<GarbageOrderDto>> {
    const g = encodeURIComponent(groupId);
    const o = encodeURIComponent(orderId);
    return this.http
      .post<Result<GarbageOrderDto>>(`${this.apiUrl}/garbage-group/${g}/order/${o}/utilization-fee/payment`, {})
      .pipe(tap((res) => {
        if (res.resultModel) {
          this.upsertOrder(res.resultModel);
        }
      }));
  }

  createOrder(groupId: string, payload: CreateGarbageOrderRequest): Observable<Result<GarbageOrderDto>> {
    const g = encodeURIComponent(groupId);
    return this.http
      .post<Result<GarbageOrderDto>>(`${this.apiUrl}/garbage-group/${g}/order`, payload)
      .pipe(tap((res) => {
        if (res.resultModel) {
          this.upsertOrder(res.resultModel);
        }
      }));
  }

  ensureMyOrders(pageNumber: number, pageSize: number): Observable<PaginatedResult<GarbageOrderDto[]>> {
    if (this.loadedSignal()) {
      return of({
        resultModel: this.ordersSignal(),
        errorCode: null,
        errorMessage: null,
        pager: this.pagerSignal()
      });
    }
    return this.getMyOrders(pageNumber, pageSize);
  }

  findOrderById(orderId: string): GarbageOrderDto | null {
    return this.ordersSignal().find(order => order.id === orderId) ?? null;
  }

  private setCache(res: PaginatedResult<GarbageOrderDto[]>): void {
    this.ordersSignal.set(res.resultModel ?? []);
    this.pagerSignal.set(res.pager ?? null);
    this.loadedSignal.set(true);
  }

  private upsertOrder(order: GarbageOrderDto): void {
    this.ordersSignal.update((items) => {
      const index = items.findIndex(item => item.id === order.id);
      if (index === -1) {
        return [order, ...items];
      }
      const copy = items.slice();
      copy[index] = order;
      return copy;
    });
  }

  getGroupOrders(
    groupId: string,
    pageNumber: number,
    pageSize: number,
    filter?: GarbageOrderFilterRequest
  ): Observable<PaginatedResult<GarbageOrderDto[]>> {
    const params = new HttpParams()
      .set('pageNumber', String(pageNumber))
      .set('pageSize', String(pageSize));

    const payload = {
      fromDate: filter?.fromDate ?? null,
      toDate: filter?.toDate ?? null,
      pickupOption: filter?.pickupOption ?? null,
      statuses: filter?.statuses ?? []
    };

    const encodedGroupId = encodeURIComponent(groupId);

    return this.http.post<PaginatedResult<GarbageOrderDto[]>>(
      `${this.apiUrl}/garbage-group/${encodedGroupId}/orders/filter`,
      payload,
      { params }
    );
  }

  calculateOrderCost(
    groupId: string,
    payload: CalculateGarbageOrderRequest
  ): Observable<Result<GarbageOrderCostDto>> {
    const encodedGroupId = encodeURIComponent(groupId);
    return this.http.post<Result<GarbageOrderCostDto>>(
      `${this.apiUrl}/garbage-group/${encodedGroupId}/order/calculate`,
      payload
    );
  }
}
