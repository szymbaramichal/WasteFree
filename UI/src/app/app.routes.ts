import { PortalComponent } from './portal/portal.component';
import { HomeComponent } from './home/home.component';
import { Routes } from '@angular/router';
import { AuthComponent } from './auth/auth.component';

export const routes: Routes = [
    { path: '', component: HomeComponent },
    { path: 'portal', component: PortalComponent },
    { path: 'auth', component: AuthComponent}       
];
