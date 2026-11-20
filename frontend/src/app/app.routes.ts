import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/auth.guard';
import { ShellComponent } from './layout/shell.component';
import { LoginComponent } from './pages/login/login.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { TransactionsComponent } from './pages/transactions/transactions.component';
import { TransactionFormComponent } from './pages/transaction-form/transaction-form.component';
import { ProjectDetailComponent } from './pages/project-detail/project-detail.component';
import { ProjectsComponent } from './pages/projects/projects.component';
import { DocumentsComponent } from './pages/documents/documents.component';
import { ImportComponent } from './pages/import/import.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent, canActivate: [guestGuard] },
  {
    path: '',
    component: ShellComponent,
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      { path: 'dashboard', component: DashboardComponent },
      { path: 'transactions', component: TransactionsComponent },
      { path: 'transactions/new', component: TransactionFormComponent },
      { path: 'transactions/:id/edit', component: TransactionFormComponent },
      { path: 'projects', component: ProjectsComponent },
      { path: 'projects/:id', component: ProjectDetailComponent },
      { path: 'quotations', component: DocumentsComponent, canDeactivate: [(c: DocumentsComponent) => c.canLeave()] },
      { path: 'import', component: ImportComponent }
    ]
  },
  { path: '**', redirectTo: 'dashboard' }
];
