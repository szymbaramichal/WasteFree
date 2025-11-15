import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { CurrentUserService } from '@app/services/current-user.service';
import { UserRole } from '@app/_models/user';

export const garbageAdminGuard: CanActivateFn = () => {
  const router = inject(Router);
  const currentUser = inject(CurrentUserService);
  const user = currentUser.user();

  if (user?.role === UserRole.GarbageAdmin) {
    return true;
  }

  router.navigate(['/portal']);
  return false;
};
