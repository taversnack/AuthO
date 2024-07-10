import { NgModule, inject } from '@angular/core';
import { CanMatchFn, RouterModule, Routes } from '@angular/router';
import { UserRole } from '../auth/models/user-roles';
import { AuthService } from '../auth/services/auth.service';

const canMatchRole = (role: UserRole): CanMatchFn => () => inject(AuthService).userHasRoleAndRoleIsEnabled(role);

const routes: Routes = [
  // Load module based on role to prevent larger than necessary bundles being downloaded.
  {
    path: '',
    canMatch: [canMatchRole(UserRole.SuperUser)],
    loadChildren: () => import('./super-user/super-user.module').then(m => m.SuperUserModule)
  },
  {
    path: '',
    canMatch: [canMatchRole(UserRole.TenantAdmin)],
    loadChildren: () => import('./tenant-admin/tenant-admin.module').then(m => m.TenantAdminModule)
  },
  {
    path: '',
    canMatch: [canMatchRole(UserRole.LockerBankAdmin)],
    loadChildren: () => import('../v2/pages/locker-bank-admin/v2-locker-bank-admin.module').then(m => m.V2LockerBankAdminModule)
  },
  {
    path: '',
    canMatch: [canMatchRole(UserRole.Installer)],
    loadChildren: () => import('./installer/installer.module').then(m => m.InstallerModule)
  },
  // {
  //   path: '',
  //   title: 'User Dashboard',
  //   component: UserDashboardPageComponent,
  // },
  {
    path: '',
    pathMatch: 'full',
    redirectTo: '/',
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class AdminRoutingModule {}
