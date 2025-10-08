import { PortalComponent } from './portal/portal.component';
import { HomeComponent } from './home/home.component';
import { Routes } from '@angular/router';
import { AuthComponent } from './auth/auth.component';
import { authGuard } from './guards/auth.guard';
import { ActivationComponent } from './activation/activation.component';
import { UrlSegment, UrlMatchResult } from '@angular/router';
import { WalletComponent } from './wallet/wallet.component';
import { InboxComponent } from './inbox/inbox.component';
import { PortalHomeComponent } from './portal-home/portal-home.component';
import { GroupsComponent } from './groups/groups.component';
import { GroupsManagementComponent } from './groups-management/groups-management.component';
import { ProfileComponent } from './profile/profile.component';
import { GroupPanelComponent } from './group-panel/group-panel.component';
import { groupResolver } from './resolvers/group.resolver';

function activateAccountMatcher(segments: UrlSegment[]): UrlMatchResult | null {
    if (!segments || segments.length === 0) return null;
    if (segments[0].path !== 'activate-account') return null;

    const tokenSegments = segments.slice(1);
    if (tokenSegments.length === 0) return null;

    const token = tokenSegments.map(s => s.path).join('/');

    return {
        consumed: segments,
        posParams: {
            token: new UrlSegment(token, {})
        }
    };
}

export const routes: Routes = [
    { path: '', component: HomeComponent },
    {
        path: 'portal',
        component: PortalComponent,
        data: { showTopbar: true },
        canActivate: [authGuard],
        children: [
            { path: '', component: PortalHomeComponent },
            { path: 'wallet', component: WalletComponent },
            { path: 'inbox', component: InboxComponent },
            { path: 'groups', component: GroupsComponent },
            { path: 'groups/manage', component: GroupsManagementComponent },
            { path: 'groups/:groupId', component: GroupPanelComponent, resolve: { group: groupResolver } },
            { path: 'profile', component: ProfileComponent },

        ]
    },
    { path: 'auth', component: AuthComponent },
    { matcher: activateAccountMatcher, component: ActivationComponent }
];