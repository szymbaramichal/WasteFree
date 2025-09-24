import { PortalComponent } from './portal/portal.component';
import { PortalHomeComponent } from './portal/portal-home.component';
import { HomeComponent } from './home/home.component';
import { Routes, UrlSegment, UrlMatchResult } from '@angular/router';
import { AuthComponent } from './auth/auth.component';
import { authGuard } from './guards/auth.guard';
import { ActivationComponent } from './activation/activation.component';

// Custom matcher: if URL starts with 'activate-account', capture the rest of the path (including slashes)
export function matchActivateAccount(segments: UrlSegment[]): UrlMatchResult | null {
    if (!segments || segments.length === 0) return null;
    if (segments[0].path !== 'activate-account') return null;

    // If there's no additional segment, still match (component can read query param)
    if (segments.length === 1) {
        return { consumed: segments };
    }

    // Join remaining segments with '/' to reconstruct the full token
    const token = segments.slice(1).map(s => s.path).join('/');
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
            { path: 'wallet', loadComponent: () => import('./wallet/wallet.component').then(m => m.WalletComponent), data: { showTopbar: true } }
        ]
    },
    { path: 'auth', component: AuthComponent },
    // Use matcher so tokens containing slashes/+/= are accepted in the path
    { matcher: matchActivateAccount, component: ActivationComponent }
];
