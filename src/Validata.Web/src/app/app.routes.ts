import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'login', loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent) },
  { path: 'register', loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent) },
  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
  },
  {
    path: 'admin',
    canActivate: [authGuard],
    loadComponent: () => import('./features/dashboard/admin-dashboard.component').then(m => m.AdminDashboardComponent)
  },
  {
    path: 'documents',
    canActivate: [authGuard],
    loadComponent: () => import('./features/documents/document-list/document-list.component').then(m => m.DocumentListComponent)
  },
  {
    path: 'documents/upload',
    canActivate: [authGuard],
    loadComponent: () => import('./features/documents/document-upload/document-upload.component').then(m => m.DocumentUploadComponent)
  },
  {
    path: 'audit',
    canActivate: [authGuard],
    loadComponent: () => import('./features/audit/audit-log.component').then(m => m.AuditLogComponent)
  },
  {
    path: 'gdpr',
    canActivate: [authGuard],
    loadComponent: () => import('./features/gdpr/gdpr-portal.component').then(m => m.GDPRPortalComponent)
  },
  {
    path: 'reports',
    canActivate: [authGuard],
    loadComponent: () => import('./features/reports/sla-reports.component').then(m => m.SlaReportsComponent)
  },
  { path: '**', redirectTo: '/login' }
];
