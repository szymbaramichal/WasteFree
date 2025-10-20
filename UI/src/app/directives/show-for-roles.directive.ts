import { Directive, Input, TemplateRef, ViewContainerRef } from '@angular/core';
import { CurrentUserService } from '@app/services/current-user.service';
import { UserRole } from '@app/_models/user';

@Directive({
  selector: '[appShowForRoles]',
  standalone: true
})
export class ShowForRolesDirective {
  private allowedRoles: ReadonlySet<UserRole> = new Set();
  private hasView = false;

  constructor(
    private templateRef: TemplateRef<unknown>,
    private viewContainer: ViewContainerRef,
    private currentUser: CurrentUserService
  ) {}

  @Input({ required: true })
  set appShowForRoles(value: UserRole | UserRole[]) {
    const normalized = Array.isArray(value) ? value : [value];
    this.allowedRoles = new Set(normalized);
    this.updateView();
  }

  private updateView() {
    const user = this.currentUser.user();
    const shouldShow = user ? this.allowedRoles.has(user.role) : false;

    if (shouldShow && !this.hasView) {
      this.viewContainer.createEmbeddedView(this.templateRef);
      this.hasView = true;
    } else if (!shouldShow && this.hasView) {
      this.viewContainer.clear();
      this.hasView = false;
    }
  }
}
