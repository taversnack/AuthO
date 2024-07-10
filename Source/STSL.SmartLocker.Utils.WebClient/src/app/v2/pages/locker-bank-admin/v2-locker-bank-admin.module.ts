import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';

import { DialogModule } from '@angular/cdk/dialog';
import { MaterialModule } from 'src/app/material/material.module';
import { LayoutComponent } from 'src/app/shared/components/layout/layout.component';
import { AdminCommonModule } from 'src/app/admin/admin-common/admin-common.module';
import { V2LockerBankAdminRoutingModule } from './v2-locker-bank-admin-routing.module';
import { LockerBankOverviewComponent } from './locker-assignment-and-status/locker-banks/locker-bank-overview/locker-bank-overview.component';
import { LockerBankSummaryCardComponent } from './locker-assignment-and-status/locker-banks/locker-bank-summary-card/locker-bank-summary-card.component';
import { LockerSummaryCardComponent } from './locker-assignment-and-status/lockers/locker-summary-card/locker-summary-card-component';
import { LockerBankAdminOverviewComponent } from './locker-assignment-and-status/locker-bank-admin-overview.component';
import { LockersOverviewComponent } from './locker-assignment-and-status/lockers/lockers-overview/lockers-overview.component';
import { SearchBarComponentV2 } from '../../shared/components/search-bar/search-bar.component';
import { LockerSummaryComponent } from './locker-assignment-and-status/lockers/locker-summary/locker-summary.component';
import { LocksAuditTableComponent } from './locker-assignment-and-status/lockers/locker-summary/lock-audits-table/lock-audits-table.component';
import { AssignCardHoldersComponent } from './locker-assignment-and-status/lockers/locker-summary/assign-users/assign-card-holders.component';


@NgModule({
  declarations: [
    LockerBankAdminOverviewComponent,
    // Locker Bank Components
    LockerBankOverviewComponent,
    LockerBankSummaryCardComponent,
    // Locker Components
    LockersOverviewComponent,
    LockerSummaryCardComponent,
    LockerSummaryComponent,
    LocksAuditTableComponent,
    AssignCardHoldersComponent
  ],
  imports: [
    CommonModule,
    V2LockerBankAdminRoutingModule,
    DialogModule,
    MaterialModule,
    LayoutComponent,
    AdminCommonModule,
    SearchBarComponentV2,
  ]
})
export class V2LockerBankAdminModule { }
