import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Result } from '../_models/result';
import { GarbageGroup, GarbageGroupInfo, RegisterGarbageGroupRequest } from '../_models/garbageGroups';

@Injectable({
  providedIn: 'root'
})
export class GarbageGroupService {
  private apiUrl = `${environment.apiUrl}/garbage-groups`;

  constructor(private http: HttpClient) {}

  register(data: RegisterGarbageGroupRequest): Observable<Result<GarbageGroup>> {
    return this.http.post<Result<GarbageGroup>>(`${this.apiUrl}/register`, data);
  }

  list(): Observable<Result<GarbageGroupInfo[]>> {
    return this.http.get<Result<GarbageGroupInfo[]>>(`${this.apiUrl}/list`);
  }
}
