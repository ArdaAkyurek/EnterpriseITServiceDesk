import { Routes } from '@angular/router';

import { Login } from './features/auth/login/login';
import { Dashboard } from './features/dashboard/dashboard';
import { Layout } from './core/layout/layout';
import { Assets } from './assets/assets';

export const routes: Routes = [
  {
    path: '',
    component: Login
  },
  {
    path: 'dashboard',
    component: Layout,
    children: [
      {
        path: '',
        component: Dashboard
      },
      {
        path: 'assets',
        component: Assets
      }
    ]
  }
];