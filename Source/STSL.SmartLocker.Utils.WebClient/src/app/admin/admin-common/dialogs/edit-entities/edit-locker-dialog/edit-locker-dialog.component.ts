import { Component, inject } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { AdminApiService } from 'src/app/admin/services/admin-api.service';
import { DialogBase } from 'src/app/dialog/directives/dialog-component-base.directive';
import { EntityId } from 'src/app/shared/models/common';
import { ICreateLockerDTO, ILockerDTO, IUpdateLockerDTO } from 'src/app/shared/models/locker';
import { EditEntityDialogState } from '../../../models/entity-forms';

export interface IEditLockerDialogData {
  entity?: ILockerDTO;
  lockerBankId: EntityId;
  allowSmartLocksOnly?: boolean;
}

@Component({
  selector: 'admin-edit-locker-dialog',
  templateUrl: './edit-locker-dialog.component.html',
})
export class EditLockerDialogComponent extends DialogBase<EditLockerDialogComponent, IEditLockerDialogData, ILockerDTO> {

  private readonly adminApiService = inject(AdminApiService);

  private readonly lockerSubject = new BehaviorSubject<ILockerDTO | undefined>(this.data?.entity);
  readonly locker$ = this.lockerSubject.asObservable();

  dialogState = EditEntityDialogState.EditingForm;
  readonly EditEntityDialogState = EditEntityDialogState;

  allowSmartLocksOnly = this.data?.allowSmartLocksOnly || false;

  closeValue?: ILockerDTO;

  createOrEditLocker(locker: ICreateLockerDTO | IUpdateLockerDTO) {
    const lockerBankId = this.data?.lockerBankId;
    const currentLocker = this.lockerSubject.getValue();

    if(!lockerBankId) {
      throw new Error('Locker Bank Id was not provided. Please notify your account administrator');
    }

    const lockerToSave = { ...locker, lockerBankId: lockerBankId };

    const error = (err: Error) => { this.notifyFailedSave(); throw err };

    if(currentLocker) {
      this.adminApiService.Lockers.updateSingle(currentLocker.id, lockerToSave).subscribe(
        {
          next: updated => updated ? this.notifySuccessfulSave({ ...currentLocker, ...lockerToSave }) : this.notifyFailedSave(),
          error,
        }
      );
    } else {
      this.adminApiService.Lockers.createSingle(lockerToSave).subscribe(
        {
          next: created => created ? this.notifySuccessfulSave(created) : this.notifyFailedSave(),
          error,
        }
      );
    }
    this.dialogState = EditEntityDialogState.SavingData;
  }

  notifySuccessfulSave(locker: ILockerDTO) {
    this.dialogState = EditEntityDialogState.SaveSuccessful;
    this.closeValue = locker;
  }

  notifyFailedSave() {
    this.dialogState = EditEntityDialogState.SaveFailed;
  }
}
