import { Routes } from '@angular/router';
import { Login } from './features/login/login';
import { Products } from './features/products/products';
import { ReservationPage } from './features/reservation/reservation';
import { Alerts } from './features/alerts/alerts';
export const routes: Routes = [
    { path: 'login', component: Login },
    { path: 'products', component: Products },
    { path: 'reserve', component: ReservationPage },
    { path: 'alerts', component: Alerts },
    { path: '', redirectTo: '/login', pathMatch: 'full' }
];