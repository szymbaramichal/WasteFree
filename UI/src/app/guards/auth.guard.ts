import { Injectable } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { CurrentUserService } from '../services/current-user.service';

export const authGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const currentUser = inject(CurrentUserService);
  const token = localStorage.getItem('authToken');

  if (token) return true;
  if (currentUser.user()) return true;

  router.navigate(['/auth']);
  return false;
};
