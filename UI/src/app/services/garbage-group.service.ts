import { Injectable, inject } from '@angular/core';
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
  private http = inject(HttpClient);

  register(data: RegisterGarbageGroupRequest): Observable<Result<GarbageGroup>> {
    return this.http.post<Result<GarbageGroup>>(`${this.apiUrl}/register`, data);
  }

  list(): Observable<Result<GarbageGroupInfo[]>> {
    return this.http.get<Result<GarbageGroupInfo[]>>(`${this.apiUrl}/list`);
  }

  details(id: string): Observable<Result<GarbageGroup>> {
    const safeId = encodeURIComponent(id);
    return this.http.get<Result<GarbageGroup>>(`${this.apiUrl}/${safeId}`);
  }

  inviteUser(groupId: string, userName: string): Observable<Result<any>> {
    return this.http.post<Result<any>>(`${this.apiUrl}/invite`, { groupId, userName });
  }

  removeUser(groupId: string, userId: string): Observable<Result<any>> {
    const g = encodeURIComponent(groupId);
    const u = encodeURIComponent(userId);
    return this.http.delete<Result<any>>(`${this.apiUrl}/${g}/${u}`);
  }
}
