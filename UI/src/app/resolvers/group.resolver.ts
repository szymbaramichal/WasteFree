import { inject } from '@angular/core';
import { ResolveFn } from '@angular/router';
import { GarbageGroup } from '../_models/garbageGroups';
import { GarbageGroupService } from '../services/garbage-group.service';
import { catchError, map, of } from 'rxjs';

export const groupResolver: ResolveFn<GarbageGroup | null> = (route) => {
  const id = route.paramMap.get('groupId');
  const svc = inject(GarbageGroupService);
  if (!id) {
    try { console.warn('Attempted to open group panel without id'); } catch {}
    return of(null);
  }
  return svc.details(id).pipe(
    map(res => res.resultModel ?? null),
    catchError(err => {
      const apiMsg = err?.error?.errorMessage;
      try { console.error('Group resolver failed', { id, apiMsg, err }); } catch {}
      return of(null);
    })
  );
};
