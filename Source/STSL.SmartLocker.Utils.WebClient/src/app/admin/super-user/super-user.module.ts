import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';

import { DialogModule } from '@angular/cdk/dialog';
import { ReactiveFormsModule } from '@angular/forms';
import { MaterialModule } from 'src/app/material/material.module';
import { ApiDataTableComponent } from 'src/app/shared/components/api-data-table/api-data-table.component';
import { LayoutComponent } from 'src/app/shared/components/layout/layout.component';
import { TextInputComponent } from 'src/app/shared/components/text-input/text-input.component';
import { SearchBarComponent } from '../../shared/components/search-bar/search-bar.component';
import { AdminCommonModule } from '../admin-common/admin-common.module';
import { EditTenantFormComponent } from './components/edit-tenant-form/edit-tenant-form.component';
import { CardHoldersAndCardCredentialsOverviewPageComponent } from './pages/card-holders-and-card-credentials-overview-page/card-holders-and-card-credentials-overview-page.component';
import { LockerBankAdminsOverviewPageComponent } from './pages/locker-bank-admins-overview-page/locker-bank-admins-overview-page.component';
import { LockerBanksAndLockersOverviewPageComponent } from './pages/locker-banks-and-lockers-overview-page/locker-banks-and-lockers-overview-page.component';
import { LockerStatusesOverviewPageComponent } from './pages/locker-statuses-overview-page/locker-statuses-overview-page.component';
import { LocksOverviewPageComponent } from './pages/locks-overview-page/locks-overview-page.component';
import { SuperUserDashboardPageComponent } from './pages/super-user-dashboard-page/super-user-dashboard-page.component';
import { TenantsOverviewPageComponent } from './pages/tenants-overview-page/tenants-overview-page.component';
import { SuperUserRoutingModule } from './super-user-routing.module';

@NgModule({
  declarations: [
    SuperUserDashboardPageComponent,
    TenantsOverviewPageComponent,
    EditTenantFormComponent,
    LockerBanksAndLockersOverviewPageComponent,
    CardHoldersAndCardCredentialsOverviewPageComponent,
    LockerBankAdminsOverviewPageComponent,
    LockerStatusesOverviewPageComponent,
    LocksOverviewPageComponent,
  ],
  imports: [
    CommonModule,
    SuperUserRoutingModule,
    ReactiveFormsModule,
    DialogModule,
    MaterialModule,
    LayoutComponent,
    AdminCommonModule,
    SearchBarComponent,
    TextInputComponent,
    ApiDataTableComponent,
  ],
})
export class SuperUserModule {}
