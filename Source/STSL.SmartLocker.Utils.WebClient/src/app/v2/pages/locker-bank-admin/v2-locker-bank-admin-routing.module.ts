import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LockerBankAdminOverviewComponent } from './locker-assignment-and-status/locker-bank-admin-overview.component';

const routes: Routes = [
  {
    path: '',
    redirectTo: 'locker-statuses-and-audits',
    pathMatch: 'full'
  },
  {
    path: 'locker-statuses-and-audits',
    title: 'Manage Lockers & View Statuses',
    component: LockerBankAdminOverviewComponent
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class V2LockerBankAdminRoutingModule { }
