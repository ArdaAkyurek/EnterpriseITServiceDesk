import { Routes } from '@angular/router';

import { Login } from './features/auth/login/login';
import { Dashboard } from './features/dashboard/dashboard';
import { Layout } from './core/layout/layout';
import { Assets } from './assets/assets';
import { authGuard } from './core/auth.guard';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    component: Login
  },
  {
    path: 'dashboard',
    component: Layout,
    canActivate: [authGuard],
    canActivateChild: [authGuard],
    children: [
      {
        path: '',
        component: Dashboard
      },
      {
        path: 'assets',
        component: Assets,
        data: { roles: ['Admin', 'Technician'] }
      }
    ]
  },
  { path: '**', redirectTo: 'dashboard' }
];
