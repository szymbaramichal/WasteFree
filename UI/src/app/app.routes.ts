import { PortalComponent } from '@components/shared/portal/portal.component';
import { HomeComponent } from '@components/shared/home/home.component';
import { Routes } from '@angular/router';
import { AuthComponent } from '@components/shared/auth/auth.component';
import { authGuard } from '@app/guards/auth.guard';
import { ActivationComponent } from '@components/shared/activation/activation.component';
import { UrlSegment, UrlMatchResult } from '@angular/router';
import { WalletComponent } from '@components/shared/wallet/wallet.component';
import { InboxComponent } from '@components/shared/inbox/inbox.component';
import { PortalHomeComponent } from '@components/shared/portal-home/portal-home.component';
import { GroupsComponent } from '@components/user/groups/groups.component';
import { GroupsManagementComponent } from '@components/user/groups-management/groups-management.component';
import { ProfileComponent } from '@components/shared/profile/profile.component';
import { GroupPanelComponent } from '@components/shared/group-panel/group-panel.component';
import { groupResolver } from '@app/resolvers/group.resolver';
import { NotFoundComponent } from '@components/shared/not-found/not-found.component';
import { PickupOrderComponent } from '@components/shared/pickup-order/pickup-order.component';
import { MyPickupsComponent } from '@components/user/my-pickups/my-pickups.component';
import { OrderDetailsComponent } from '@components/user/order-details/order-details.component';

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
            { path: 'pickup-order', component: PickupOrderComponent },
            { path: 'my-pickups', component: MyPickupsComponent },
            { path: 'my-pickups/:orderId', component: OrderDetailsComponent },
            { path: 'groups', component: GroupsComponent },
            { path: 'groups/manage', component: GroupsManagementComponent },
            { path: 'groups/:groupId', component: GroupPanelComponent, resolve: { group: groupResolver } },
            { path: 'profile', component: ProfileComponent },

        ]
    },
    { path: 'auth', component: AuthComponent },
    { matcher: activateAccountMatcher, component: ActivationComponent },
    { path: '**', component: NotFoundComponent }
];