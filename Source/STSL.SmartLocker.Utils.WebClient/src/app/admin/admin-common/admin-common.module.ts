import { DialogModule } from '@angular/cdk/dialog';
import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MaterialModule } from 'src/app/material/material.module';
import { CountdownComponent } from 'src/app/shared/components/countdown/countdown.component';
import { DataTableComponent } from 'src/app/shared/components/data-table/data-table.component';
import { LayoutComponent } from 'src/app/shared/components/layout/layout.component';
import { SearchBarComponent } from 'src/app/shared/components/search-bar/search-bar.component';
import { TextInputComponent } from 'src/app/shared/components/text-input/text-input.component';
import { ApiDataTableComponent } from "../../shared/components/api-data-table/api-data-table.component";
import { EditCardCredentialFormComponent } from './components/entity-forms/edit-card-credential-form/edit-card-credential-form.component';
import { EditCardHolderFormComponent } from './components/entity-forms/edit-card-holder-form/edit-card-holder-form.component';
import { EditLocationFormComponent } from './components/entity-forms/edit-location-form/edit-location-form.component';
import { EditLockFormComponent } from './components/entity-forms/edit-lock-form/edit-lock-form.component';
import { EditLockerBankFormComponent } from './components/entity-forms/edit-locker-bank-form/edit-locker-bank-form.component';
import { EditLockerFormComponent } from './components/entity-forms/edit-locker-form/edit-locker-form.component';
import { CardCredentialsViewComponent } from './components/entity-views/card-credentials-view/card-credentials-view.component';
import { CardHoldersViewComponent } from './components/entity-views/card-holders-view/card-holders-view.component';
import { LocationsViewComponent } from './components/entity-views/locations-view/locations-view.component';
import { LockerAuditsViewComponent } from './components/entity-views/locker-audits-view/locker-audits-view.component';
import { LockerBankAdminsViewComponent } from './components/entity-views/locker-bank-admins-view/locker-bank-admins-view.component';
import { LockerBanksViewComponent } from './components/entity-views/locker-banks-view/locker-banks-view.component';
import { LockerLeasesViewComponent } from './components/entity-views/locker-leases-view/locker-leases-view.component';
import { LockerStatusesViewComponent } from './components/entity-views/locker-statuses-view/locker-statuses-view.component';
import { LockersAndLocksViewComponent } from './components/entity-views/lockers-and-locks-view/lockers-and-locks-view.component';
import { LockersViewComponent } from './components/entity-views/lockers-view/lockers-view.component';
import { LocksViewComponent } from './components/entity-views/locks-view/locks-view.component';
import { EditCardCredentialDialogComponent } from './dialogs/edit-entities/edit-card-credential-dialog/edit-card-credential-dialog.component';
import { EditCardHolderDialogComponent } from './dialogs/edit-entities/edit-card-holder-dialog/edit-card-holder-dialog.component';
import { EditLocationDialogComponent } from './dialogs/edit-entities/edit-location-dialog/edit-location-dialog.component';
import { EditLockDialogComponent } from './dialogs/edit-entities/edit-lock-dialog/edit-lock-dialog.component';
import { EditLockerBankDialogComponent } from './dialogs/edit-entities/edit-locker-bank-dialog/edit-locker-bank-dialog.component';
import { EditLockerDialogComponent } from './dialogs/edit-entities/edit-locker-dialog/edit-locker-dialog.component';
import { EnrolCardCredentialDialogComponent } from './dialogs/edit-entities/enrol-card-credential-dialog/enrol-card-credential-dialog.component';
import { SelectCardCredentialsDialogComponent } from './dialogs/view-entities/select-card-credentials-dialog/select-card-credentials-dialog.component';
import { SelectCardHoldersDialogComponent } from './dialogs/view-entities/select-card-holders-dialog/select-card-holders-dialog.component';
import { SelectCardHoldersOrCardCredentialsDialogComponent } from './dialogs/view-entities/select-card-holders-or-card-credentials-dialog/select-card-holders-or-card-credentials-dialog.component';
import { SelectCardsFromListDialogComponent } from './dialogs/view-entities/select-cards-from-list-dialog/select-cards-from-list-dialog.component';
import { SelectLockerBanksDialogComponent } from './dialogs/view-entities/select-locker-banks-dialog/select-locker-banks-dialog.component';
import { SelectLockersDialogComponent } from './dialogs/view-entities/select-lockers-dialog/select-lockers-dialog.component';
import { SelectLocksDialogComponent } from './dialogs/view-entities/select-locks-dialog/select-locks-dialog.component';
import { ViewLockerAuditsDialogComponent } from './dialogs/view-entities/view-locker-audits-dialog/view-locker-audits-dialog.component';
import { ViewLockerLeasesDialogComponent } from './dialogs/view-entities/view-locker-leases-dialog/view-locker-leases-dialog.component';
import { ViewReferenceImageDialogComponent } from './dialogs/view-entities/view-reference-image-dialog/view-reference-image-dialog.component';
import { ImageInputComponent } from 'src/app/dialog/components/image-input/image-input.component';
import { EditReferenceImageComponent } from './components/entity-forms/edit-reference-image/edit-reference-image.component';
import { EditReferenceImageDialogComponent } from './dialogs/edit-entities/edit-reference-image-dialog/edit-reference-image-dialog.component';

const publicComponents = [
  LocationsViewComponent,
  LockerBanksViewComponent,
  LockerBankAdminsViewComponent,
  LockersViewComponent,
  LockerStatusesViewComponent,
  LockerAuditsViewComponent,
  CardHoldersViewComponent,
  CardCredentialsViewComponent,
  LockersAndLocksViewComponent,
  LocksViewComponent,
  LockerLeasesViewComponent,

  SelectCardCredentialsDialogComponent,
  SelectCardHoldersDialogComponent,
  SelectCardHoldersOrCardCredentialsDialogComponent,
  SelectLockersDialogComponent,
  SelectLockerBanksDialogComponent,
  SelectLocksDialogComponent,
  SelectCardsFromListDialogComponent,
  ViewLockerAuditsDialogComponent,
  ViewLockerLeasesDialogComponent,
  EnrolCardCredentialDialogComponent,
  ViewReferenceImageDialogComponent,

  EditLocationFormComponent,
  EditLockerBankFormComponent,
  EditLockerFormComponent,
  EditCardHolderFormComponent,
  EditCardCredentialFormComponent,
  EditLockFormComponent,
  EditReferenceImageComponent,

  EditLocationDialogComponent,
  EditLockerBankDialogComponent,
  EditLockerDialogComponent,
  EditCardHolderDialogComponent,
  EditCardCredentialDialogComponent,
  EditLockDialogComponent,
  EditReferenceImageDialogComponent,
]

@NgModule({
    declarations: publicComponents,
    exports: publicComponents,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        FormsModule,
        DialogModule,
        MaterialModule,
        LayoutComponent,
        SearchBarComponent,
        TextInputComponent,
        DataTableComponent,
        ApiDataTableComponent,
        CountdownComponent,
        ImageInputComponent,
    ]
})
export class AdminCommonModule { }
