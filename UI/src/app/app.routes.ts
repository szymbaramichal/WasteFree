import { Routes } from '@angular/router';
import { PortalComponent } from './portal/portal.component';
import { HomeComponent } from './home/home.component';

export const routes: Routes = [
    { path: '', component: HomeComponent },
    { path: 'portal', component: PortalComponent },
];
