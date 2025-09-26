import { PortalComponent } from './portal/portal.component';
import { PortalHomeComponent } from './portal/portal-home.component';
import { HomeComponent } from './home/home.component';
import { Routes } from '@angular/router';
import { AuthComponent } from './auth/auth.component';
import { authGuard } from './guards/auth.guard';
import { ActivationComponent } from './activation/activation.component';
import { WalletComponent } from './wallet/wallet.component';
import { InboxComponent } from './inbox/inbox.component';

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
            { path: 'inbox', component: InboxComponent }
        ]
    },
    { path: 'auth', component: AuthComponent },
    { path: 'activate-account/:token', component: ActivationComponent }
];