import { PortalComponent } from './portal/portal.component';
import { HomeComponent } from './home/home.component';
import { Routes } from '@angular/router';

export const routes: Routes = [
    { path: '', component: HomeComponent },
    { path: 'portal', component: PortalComponent },
];
