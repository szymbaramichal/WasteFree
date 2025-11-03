import { Component, EffectRef, OnDestroy, OnInit, effect } from '@angular/core';
import { CurrentUserService } from '@app/services/current-user.service';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TranslatePipe } from '@app/pipes/translate.pipe';
import { NavigationEnd, Router } from '@angular/router';
import { Subject, filter, takeUntil } from 'rxjs';
import { UserRole } from '@app/_models/user';
import { ShowForRolesDirective } from '@app/directives/show-for-roles.directive';
import { GarbageAdminConsentService } from '@app/services/garbage-admin-consent.service';
import { GarbageAdminConsentModalComponent } from '@components/shared/garbage-admin-consent-modal/garbage-admin-consent-modal.component';
import { ToastrService } from 'ngx-toastr';
import { TranslationService } from '@app/services/translation.service';
import { WalletService } from '@app/services/wallet.service';

@Component({
  selector: 'app-portal',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    TranslatePipe,
    ShowForRolesDirective,
    GarbageAdminConsentModalComponent
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
  consentVisible = false;
  consentLoading = false;
  consentAccepting = false;
  consentContent: string | null = null;
  consentError: string | null = null;

  private consentEffect: EffectRef;

  constructor(
    public currentUser: CurrentUserService,
    private router: Router,
    private consentService: GarbageAdminConsentService,
    private toastr: ToastrService,
    private translation: TranslationService,
    private wallet: WalletService
  ) {
    this.consentEffect = effect(() => {
      const user = this.currentUser.user();

      if (!user || user.role !== UserRole.GarbageAdmin) {
        this.clearConsentState();
        return;
      }

      if (user.acceptedConsents) {
        this.clearConsentState();
        return;
      }

      this.consentVisible = true;

      if (!this.consentLoading && !this.consentContent && !this.consentError) {
        this.fetchConsent();
      }
    });
  }

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
    this.consentEffect.destroy();
  }

  toggleGroups(event?: Event) {
    event?.preventDefault();
    this.groupsExpanded = !this.groupsExpanded;
  }

  onConsentAccept() {
    if (this.consentAccepting || this.consentLoading || !!this.consentError) {
      return;
    }

    this.consentAccepting = true;
    this.consentService
      .acceptConsent()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.consentAccepting = false;
          const user = this.currentUser.user();
          if (user) {
            this.currentUser.setUser({ ...user, acceptedConsents: true });
          }
          this.toastr.success(this.translation.translate('consents.modal.acceptSuccess'));
          this.clearConsentState();
        },
        error: () => {
          this.consentAccepting = false;
        }
      });
  }

  onConsentReject() {
    this.toastr.info(this.translation.translate('consents.modal.rejectInfo'));
    localStorage.removeItem('authToken');
    this.currentUser.setUser(null);
    this.wallet.resetState();
    this.clearConsentState();
    this.router.navigate(['/auth']);
  }

  onConsentRetry() {
    this.fetchConsent(true);
  }

  private syncGroupsRouteState(url: string) {
    const isGroups = url.startsWith('/portal/groups');
    this.groupsRouteActive = isGroups;
    if (isGroups) {
      this.groupsExpanded = true;
    }
  }

  private fetchConsent(force = false) {
    if (this.consentLoading) {
      return;
    }

    if (!force && this.consentContent) {
      return;
    }

    this.consentLoading = true;
    this.consentError = null;
    if (force) {
      this.consentContent = null;
    }

    this.consentService
      .getConsent()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          this.consentContent = res?.resultModel ?? '';
          this.consentLoading = false;
        },
        error: () => {
          this.consentLoading = false;
        }
      });
  }

  private clearConsentState() {
    this.consentVisible = false;
    this.consentLoading = false;
    this.consentAccepting = false;
    this.consentError = null;
    this.consentContent = null;
  }
}
