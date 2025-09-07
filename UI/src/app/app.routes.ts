import { PortalComponent } from './portal/portal.component';
import { HomeComponent } from './home/home.component';
import { Routes } from '@angular/router';
import { AuthComponent } from './auth/auth.component';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
    { path: '', component: HomeComponent },
    { path: 'portal', component: PortalComponent, data: { showTopbar: true }, canActivate: [authGuard] },
    { path: 'auth', component: AuthComponent}       
];
