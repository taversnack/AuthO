import { Component, inject } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { AdminApiService } from 'src/app/admin/services/admin-api.service';
import { DialogBase } from 'src/app/dialog/directives/dialog-component-base.directive';
import { ICardHolderDTO, ICreateCardHolderDTO, IUpdateCardHolderDTO } from 'src/app/shared/models/card-holder';
import { EditEntityDialogState } from '../../../models/entity-forms';

export interface IEditCardHolderDialogData {
  entity?: ICardHolderDTO;
}

@Component({
  selector: 'admin-edit-card-holder-dialog',
  templateUrl: './edit-card-holder-dialog.component.html',
})
export class EditCardHolderDialogComponent extends DialogBase<EditCardHolderDialogComponent, IEditCardHolderDialogData, ICardHolderDTO> {

  private readonly adminApiService = inject(AdminApiService);

  private readonly cardHolderSubject = new BehaviorSubject<ICardHolderDTO | undefined>(this.data?.entity);
  readonly cardHolder$ = this.cardHolderSubject.asObservable();

  dialogState = EditEntityDialogState.EditingForm;
  readonly EditEntityDialogState = EditEntityDialogState;

  closeValue?: ICardHolderDTO;

  createOrEditCardHolder(cardHolder: ICreateCardHolderDTO | IUpdateCardHolderDTO) {
    const currentCardHolder = this.cardHolderSubject.getValue();

    const error = (err: Error) => { this.notifyFailedSave(); throw err };

    if(currentCardHolder) {
      this.adminApiService.CardHolders.updateSingle(currentCardHolder.id, cardHolder).subscribe(
        {
          next: updated => updated ? this.notifySuccessfulSave({ ...currentCardHolder, ...cardHolder }) : this.notifyFailedSave(),
          error,
        }
      );
    } else {
      this.adminApiService.CardHolders.createSingle(cardHolder).subscribe(
        {
          next: created => created ? this.notifySuccessfulSave(created) : this.notifyFailedSave(),
          error,
        }
      );
    }
    this.dialogState = EditEntityDialogState.SavingData;
  }

  notifySuccessfulSave(cardholder: ICardHolderDTO) {
    this.dialogState = EditEntityDialogState.SaveSuccessful;
    this.closeValue = cardholder;
  }

  notifyFailedSave() {
    this.dialogState = EditEntityDialogState.SaveFailed;
  }
}
