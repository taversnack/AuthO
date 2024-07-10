import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LockerStatusesOverviewPageComponent } from './pages/locker-statuses-overview-page/locker-statuses-overview-page.component';

const routes: Routes = [
  // {
  //   path: '',
  //   title: 'Dashboard',
  //   component: LockerBankAdminDashboardPageComponent
  // },
  {
    path: '',
    redirectTo: 'locker-statuses-and-audits',
    pathMatch: 'full'
  },
  // {
  //   path: 'locker-banks-and-lockers',
  //   title: 'Locker Banks & Lockers',
  //   component: LockerBanksAndLockersOverviewPageComponent
  // },
  {
    path: 'locker-statuses-and-audits',
    title: 'Manage Lockers & View Statuses',
    component: LockerStatusesOverviewPageComponent
  },
  // {
  //   path: 'card-holders-and-credentials',
  //   title: 'Card Holders and Credentials',
  //   component: CardHoldersOverviewPageComponent
  // }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class LockerBankAdminRoutingModule { }
