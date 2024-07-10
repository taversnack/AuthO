import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';

import { DialogModule } from '@angular/cdk/dialog';
import { MaterialModule } from 'src/app/material/material.module';
import { LayoutComponent } from 'src/app/shared/components/layout/layout.component';
import { SearchBarComponent } from 'src/app/shared/components/search-bar/search-bar.component';
import { AdminCommonModule } from '../admin-common/admin-common.module';
import { LockerBankAdminRoutingModule } from './locker-bank-admin-routing.module';
import { LockerStatusesOverviewPageComponent } from './pages/locker-statuses-overview-page/locker-statuses-overview-page.component';


@NgModule({
  declarations: [
    // LockerBankAdminDashboardPageComponent,
    // CardHoldersOverviewPageComponent,
    // LockerBanksAndLockersOverviewPageComponent,
    LockerStatusesOverviewPageComponent
  ],
  imports: [
    CommonModule,
    LockerBankAdminRoutingModule,
    DialogModule,
    MaterialModule,
    LayoutComponent,
    AdminCommonModule,
    SearchBarComponent,
  ]
})
export class LockerBankAdminModule { }
