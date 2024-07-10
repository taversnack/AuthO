import { Component, inject } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { AdminApiService } from 'src/app/admin/services/admin-api.service';
import { DialogBase } from 'src/app/dialog/directives/dialog-component-base.directive';
import { EntityId } from 'src/app/shared/models/common';
import { ICreateLockDTO, ILockDTO, IUpdateLockDTO } from 'src/app/shared/models/lock';
import { EditEntityDialogState } from '../../../models/entity-forms';

export interface IEditLockDialogData {
  lock?: ILockDTO;
  lockerId?: EntityId;
}

@Component({
  selector: 'admin-edit-lock-dialog',
  templateUrl: './edit-lock-dialog.component.html',
})
export class EditLockDialogComponent extends DialogBase<EditLockDialogComponent, IEditLockDialogData, ILockDTO> {

  private readonly adminApiService = inject(AdminApiService);

  private readonly lockSubject = new BehaviorSubject<ILockDTO | undefined>(this.data?.lock);
  readonly lock$ = this.lockSubject.asObservable();

  dialogState = EditEntityDialogState.EditingForm;
  readonly EditEntityDialogState = EditEntityDialogState;

  closeValue?: ILockDTO;

  createOrEditLock(lock: ICreateLockDTO | IUpdateLockDTO) {
    const error = (err: Error) => { this.notifyFailedSave(); throw err; };

    const currentLock = this.lockSubject.getValue();

    const lockerId = this.data?.lockerId ?? currentLock?.lockerId ?? lock.lockerId;

    const lockToSave = { ...lock, lockerId };

    if(currentLock) {
      this.adminApiService.Locks.updateSingle(currentLock.id, lockToSave).subscribe(
        {
          next: updated => updated ? this.notifySuccessfulSave({ ...currentLock, ...lockToSave }) : this.notifyFailedSave(),
          error,
        }
      );
    } else {
      this.adminApiService.Locks.createSingle(lockToSave).subscribe(
        {
          next: created => created ? this.notifySuccessfulSave(created) : this.notifyFailedSave(),
          error,
        }
      );
    }

    this.dialogState = EditEntityDialogState.SavingData;
  }

  notifySuccessfulSave(lock: ILockDTO) {
    this.dialogState = EditEntityDialogState.SaveSuccessful;
    this.closeValue = lock;
  }

  notifyFailedSave() {
    this.dialogState = EditEntityDialogState.SaveFailed;
  }
}
