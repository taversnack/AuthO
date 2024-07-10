import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';

import { DialogModule } from 'src/app/dialog/dialog.module';
import { MaterialModule } from 'src/app/material/material.module';
import { ApiDataTableComponent } from 'src/app/shared/components/api-data-table/api-data-table.component';
import { LayoutComponent } from 'src/app/shared/components/layout/layout.component';
import { SearchBarComponent } from 'src/app/shared/components/search-bar/search-bar.component';
import { TextInputComponent } from 'src/app/shared/components/text-input/text-input.component';
import { AdminCommonModule } from '../admin-common/admin-common.module';
import { TenantAdminDashboardPageComponent } from './pages/tenant-admin-dashboard-page/tenant-admin-dashboard-page.component';
import { TenantAdminRoutingModule } from './tenant-admin-routing.module';
import { CardHoldersAndCardCredentialsOverviewPageComponent } from './pages/card-holders-and-card-credentials-overview-page/card-holders-and-card-credentials-overview-page.component';
import { LockerStatusesOverviewPageComponent } from './pages/locker-statuses-overview-page/locker-statuses-overview-page.component';
import { LockerBanksAndLockersOverviewPageComponent } from './pages/locker-banks-and-lockers-overview-page/locker-banks-and-lockers-overview-page.component';


@NgModule({
  declarations: [
    TenantAdminDashboardPageComponent,
    CardHoldersAndCardCredentialsOverviewPageComponent,
    LockerStatusesOverviewPageComponent,
    LockerBanksAndLockersOverviewPageComponent
  ],
  imports: [
    CommonModule,
    TenantAdminRoutingModule,
    // ReactiveFormsModule,
    AdminCommonModule,
    DialogModule,
    MaterialModule,
    LayoutComponent,
    SearchBarComponent,
    TextInputComponent,
    ApiDataTableComponent,
  ]
})
export class TenantAdminModule { }
