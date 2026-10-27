import { Routes } from '@angular/router';

import { Login } from './features/auth/login/login';
import { Dashboard } from './features/dashboard/dashboard';
import { Layout } from './core/layout/layout';
import { Assets } from './assets/assets';
import { authGuard } from './core/auth.guard';
import { Tickets } from './features/tickets/tickets';
import { Users } from './features/users/users';
import { TicketDetail } from './features/tickets/ticket-detail';
import { Profile } from './features/profile/profile';
import { Roles } from './features/roles/roles';
import { Departments } from './features/departments/departments';
import { AuditLogs } from './features/audit-logs/audit-logs';

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
      },
      {
        path: 'tickets',
        component: Tickets,
        data: { roles: ['Admin', 'Technician', 'Employee'] }
      },
      {
        path: 'tickets/:id',
        component: TicketDetail,
        data: { roles: ['Admin', 'Technician', 'Employee'] }
      },
      {
        path: 'users',
        component: Users,
        data: { roles: ['Admin'] }
      },
      {
        path: 'profile',
        component: Profile
      },
      {
        path: 'roles',
        component: Roles,
        data: { roles: ['Admin'] }
      },
      {
        path: 'departments',
        component: Departments,
        data: { roles: ['Admin'] }
      },
      {
        path: 'audit-logs',
        component: AuditLogs,
        data: { roles: ['Admin'] }
      }
    ]
  },
  { path: '**', redirectTo: 'dashboard' }
];
