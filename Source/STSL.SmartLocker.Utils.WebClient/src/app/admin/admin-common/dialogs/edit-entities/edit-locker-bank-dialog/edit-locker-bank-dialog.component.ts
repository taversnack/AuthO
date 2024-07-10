import { Component, inject } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { EditEntityDialogState } from 'src/app/admin/admin-common/models/entity-forms';
import { AdminApiService } from 'src/app/admin/services/admin-api.service';
import { DialogBase } from 'src/app/dialog/directives/dialog-component-base.directive';
import { EntityId } from 'src/app/shared/models/common';
import { ICreateLockerBankDTO, ILockerBankDTO, IUpdateLockerBankDTO } from 'src/app/shared/models/locker-bank';

export interface IEditLockerBankDialogData {
  entity?: ILockerBankDTO;
  locationId: EntityId;
}

@Component({
  selector: 'admin-edit-locker-bank-dialog',
  templateUrl: './edit-locker-bank-dialog.component.html',
})
export class EditLockerBankDialogComponent extends DialogBase<EditLockerBankDialogComponent, IEditLockerBankDialogData, ILockerBankDTO>  {

  private readonly adminApiService = inject(AdminApiService);

  private readonly lockerBankSubject = new BehaviorSubject<ILockerBankDTO | undefined>(this.data?.entity);
  readonly lockerBank$ = this.lockerBankSubject.asObservable();

  dialogState = EditEntityDialogState.EditingForm;
  readonly EditEntityDialogState = EditEntityDialogState;

  closeValue?: ILockerBankDTO;

  createOrEditLockerBank(lockerBank: ICreateLockerBankDTO | IUpdateLockerBankDTO) {
    const locationId = this.data?.locationId;
    const currentLockerBank = this.lockerBankSubject.getValue();

    if(!locationId) {
      throw new Error('Location Id was not provided. Please notify your account administrator');
    }

    const lockerBankToSave = { ...lockerBank, locationId };

    const error = (err: Error) => { this.notifyFailedSave(); throw err };

    if(currentLockerBank) {
      this.adminApiService.LockerBanks.updateSingle(currentLockerBank.id, lockerBankToSave).subscribe(
        {
          next: updated => updated ? this.notifySuccessfulSave({ ...currentLockerBank, ...lockerBankToSave }) : this.notifyFailedSave(),
          error,
        }
      );
    } else {
      this.adminApiService.LockerBanks.createSingle(lockerBankToSave).subscribe(
        {
          next: created => created ? this.notifySuccessfulSave(created) : this.notifyFailedSave(),
          error,
        }
      );
    }
    this.dialogState = EditEntityDialogState.SavingData;
  }

  notifySuccessfulSave(lockerBank: ILockerBankDTO) {
    this.dialogState = EditEntityDialogState.SaveSuccessful;
    this.closeValue = lockerBank;
  }

  notifyFailedSave() {
    this.dialogState = EditEntityDialogState.SaveFailed;
  }
}
