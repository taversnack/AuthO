import { Component, inject } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { AdminApiService } from 'src/app/admin/services/admin-api.service';
import { DialogBase } from 'src/app/dialog/directives/dialog-component-base.directive';
import { ICardCredentialDTO, ICreateCardCredentialDTO, IUpdateCardCredentialDTO } from 'src/app/shared/models/card-credential';
import { EntityId } from 'src/app/shared/models/common';
import { EditEntityDialogState } from '../../../models/entity-forms';

export interface IEditCardCredentialDialogData {
  cardCredential?: ICardCredentialDTO;
  cardHolderId?: EntityId;
}

@Component({
  selector: 'admin-edit-card-credential-dialog',
  templateUrl: './edit-card-credential-dialog.component.html',
})
export class EditCardCredentialDialogComponent extends DialogBase<EditCardCredentialDialogComponent, IEditCardCredentialDialogData, ICardCredentialDTO> {

  private readonly adminApiService = inject(AdminApiService);

  private readonly cardCredentialSubject = new BehaviorSubject<ICardCredentialDTO | undefined>(this.data?.cardCredential);
  readonly cardCredential$ = this.cardCredentialSubject.asObservable();

  dialogState = EditEntityDialogState.EditingForm;
  readonly EditEntityDialogState = EditEntityDialogState;

  closeValue?: ICardCredentialDTO;

  createOrEditCardCredential(cardCredential: ICreateCardCredentialDTO | IUpdateCardCredentialDTO) {
    const error = (err: Error) => { this.notifyFailedSave(); throw err };

    const currentCardCredential = this.cardCredentialSubject.getValue();

    const cardHolderId = this.data?.cardHolderId;

    if(currentCardCredential) {
      this.adminApiService.CardCredentials.updateSingle(currentCardCredential.id, { ...cardCredential, cardHolderId }).subscribe(
        {
          next: updated => updated ? this.notifySuccessfulSave({ ...currentCardCredential, ...cardCredential, cardHolderId }) : this.notifyFailedSave(),
          error,
        }
      );
    } else {
      this.adminApiService.CardCredentials.createSingle({ ...cardCredential, cardHolderId }).subscribe(
        {
          next: created => created ? this.notifySuccessfulSave(created) : this.notifyFailedSave(),
          error,
        }
      );
    }
    this.dialogState = EditEntityDialogState.SavingData;
  }

  notifySuccessfulSave(cardcredential: ICardCredentialDTO) {
    this.dialogState = EditEntityDialogState.SaveSuccessful;
    this.closeValue = cardcredential;
  }

  notifyFailedSave() {
    this.dialogState = EditEntityDialogState.SaveFailed;
  }
}
