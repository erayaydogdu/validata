import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';

export const routes: Routes = [
  { path: 'login', loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent) },
  { path: 'register', loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent) },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./shared/layout/layout.component').then(m => m.LayoutComponent),
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'admin',
        canActivate: [roleGuard('Admin')],
        loadComponent: () => import('./features/dashboard/admin-dashboard.component').then(m => m.AdminDashboardComponent)
      },
      {
        path: 'documents',
        loadComponent: () => import('./features/documents/document-list/document-list.component').then(m => m.DocumentListComponent)
      },
      {
        path: 'documents/upload',
        loadComponent: () => import('./features/documents/document-upload/document-upload.component').then(m => m.DocumentUploadComponent)
      },
      {
        path: 'audit',
        canActivate: [roleGuard('Admin', 'ComplianceOfficer')],
        loadComponent: () => import('./features/audit/audit-log.component').then(m => m.AuditLogComponent)
      },
      {
        path: 'gdpr',
        canActivate: [roleGuard('Admin', 'ComplianceOfficer')],
        loadComponent: () => import('./features/gdpr/gdpr-portal.component').then(m => m.GDPRPortalComponent)
      },
      {
        path: 'reports',
        loadComponent: () => import('./features/reports/sla-reports.component').then(m => m.SlaReportsComponent)
      }
    ]
  },
  { path: '**', redirectTo: '/login' }
];
