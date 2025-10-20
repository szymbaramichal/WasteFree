import { Component, OnDestroy, OnInit } from '@angular/core';
import { CurrentUserService } from '@app/services/current-user.service';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TranslatePipe } from '@app/pipes/translate.pipe';
import { NavigationEnd, Router } from '@angular/router';
import { Subject, filter, takeUntil } from 'rxjs';
import { UserRole } from '@app/_models/user';
import { ShowForRolesDirective } from '@app/directives/show-for-roles.directive';

@Component({
  selector: 'app-portal',
  standalone: true,
  imports: [
  CommonModule,
  RouterModule,
  TranslatePipe,
  ShowForRolesDirective
  ],
  templateUrl: './portal.component.html',
  styleUrls: ['./portal.component.css']
})
export class PortalComponent implements OnInit, OnDestroy {
  private bodyClass = 'portal-bg';
  private destroy$ = new Subject<void>();
  groupsExpanded = false;
  groupsRouteActive = false;
  userRole = UserRole;

  constructor(public currentUser: CurrentUserService, private router: Router) {}

  ngOnInit(): void {
    document.body.classList.add(this.bodyClass);
    this.syncGroupsRouteState(this.router.url);
    this.router.events
      .pipe(
        filter((event): event is NavigationEnd => event instanceof NavigationEnd),
        takeUntil(this.destroy$)
      )
      .subscribe(event => this.syncGroupsRouteState(event.urlAfterRedirects));
  }

  ngOnDestroy(): void {
    document.body.classList.remove(this.bodyClass);
    this.destroy$.next();
    this.destroy$.complete();
  }

  toggleGroups(event?: Event) {
    event?.preventDefault();
    this.groupsExpanded = !this.groupsExpanded;
  }

  private syncGroupsRouteState(url: string) {
    const isGroups = url.startsWith('/portal/groups');
    this.groupsRouteActive = isGroups;
    if (isGroups) {
      this.groupsExpanded = true;
    }
  }

}
