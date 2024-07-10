import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CardHoldersAndCardCredentialsOverviewPageComponent } from './pages/card-holders-and-card-credentials-overview-page/card-holders-and-card-credentials-overview-page.component';
import { LockerBanksAndLockersOverviewPageComponent } from './pages/locker-banks-and-lockers-overview-page/locker-banks-and-lockers-overview-page.component';
import { LockerStatusesOverviewPageComponent } from './pages/locker-statuses-overview-page/locker-statuses-overview-page.component';
import { TenantAdminDashboardPageComponent } from './pages/tenant-admin-dashboard-page/tenant-admin-dashboard-page.component';

const routes: Routes = [
  {
    path: '',
    title: 'Dashboard',
    component: TenantAdminDashboardPageComponent
  },
  {
    path: 'locker-banks-and-lockers',
    title: 'Locations, Banks & Lockers',
    component: LockerBanksAndLockersOverviewPageComponent
  },
  {
    path: 'locker-statuses-and-audits',
    title: 'Locker Statuses & Audits',
    component: LockerStatusesOverviewPageComponent
  },
  {
    path: 'card-holders-and-credentials',
    title: 'Card Holders and Credentials',
    component: CardHoldersAndCardCredentialsOverviewPageComponent
  },
  // {
  //   path: 'locker-bank-admins',
  //   title: 'Locker Bank Admins',
  //   component: LockerBankAdminsOverviewPageComponent
  // },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class TenantAdminRoutingModule { }
