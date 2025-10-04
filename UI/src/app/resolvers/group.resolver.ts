import { inject } from '@angular/core';
import { ResolveFn, Router } from '@angular/router';
import { GarbageGroup } from '../_models/garbageGroups';
import { GarbageGroupService } from '../services/garbage-group.service';
import { ToastrService } from 'ngx-toastr';
import { catchError, map, of } from 'rxjs';

export const groupResolver: ResolveFn<GarbageGroup | null> = (route) => {
  const id = route.paramMap.get('groupId');
  const svc = inject(GarbageGroupService);
  const toastr = inject(ToastrService);
  if (!id) {
    toastr.error('Invalid group id');
    return of(null);
  }
  return svc.details(id).pipe(
    map(res => res.resultModel ?? null),
    catchError(err => {
      const apiMsg = err?.error?.errorMessage;
      toastr.error(apiMsg || 'Failed to load group');
      return of(null);
    })
  );
};
