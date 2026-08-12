import { Routes } from '@angular/router';
import { Login } from './features/login/login';
import { Products } from './features/products/products';
import { ReservationPage } from './features/reservation/reservation';
import { Alerts } from './features/alerts/alerts';
import { Categories } from './features/categories/categories';
import { Suppliers } from './features/suppliers/suppliers';
import { Warehouses } from './features/warehouses/warehouses';
import { Inventory } from './features/inventory/inventory';
import { PurchaseOrders } from './features/purchase-orders/purchase-orders';
import { Users } from './features/users/users';
import { authGuard, roleGuard } from './core/auth-guard';

export const routes: Routes = [
    { path: 'login', component: Login },
    { path: 'products', component: Products, canActivate: [authGuard] },
    { path: 'reserve', component: ReservationPage, canActivate: [authGuard] },
    { path: 'inventory', component: Inventory, canActivate: [authGuard] },
    { path: 'purchase-orders', component: PurchaseOrders, canActivate: [authGuard] },
    { path: 'categories', component: Categories, canActivate: [authGuard] },
    { path: 'suppliers', component: Suppliers, canActivate: [authGuard] },
    { path: 'warehouses', component: Warehouses, canActivate: [authGuard] },
    { path: 'users', component: Users, canActivate: [authGuard, roleGuard('Administrator')] },
    { path: 'alerts', component: Alerts, canActivate: [authGuard] },
    { path: '', redirectTo: '/login', pathMatch: 'full' }
];
