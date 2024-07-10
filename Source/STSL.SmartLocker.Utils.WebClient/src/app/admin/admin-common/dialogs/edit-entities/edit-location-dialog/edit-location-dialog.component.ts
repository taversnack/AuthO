import { Component, inject } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { AdminApiService } from 'src/app/admin/services/admin-api.service';
import { DialogBase } from 'src/app/dialog/directives/dialog-component-base.directive';
import { ICreateLocationDTO, ILocationDTO, IUpdateLocationDTO } from 'src/app/shared/models/location';
import { EditEntityDialogState } from '../../../models/entity-forms';

export interface IEditLocationDialogData {
  entity?: ILocationDTO;
}

@Component({
  selector: 'admin-edit-location-dialog',
  templateUrl: './edit-location-dialog.component.html',
})
export class EditLocationDialogComponent extends DialogBase<EditLocationDialogComponent, IEditLocationDialogData, ILocationDTO> {

  private readonly adminApiService = inject(AdminApiService);

  private readonly locationSubject = new BehaviorSubject<ILocationDTO | undefined>(this.data?.entity);
  readonly location$ = this.locationSubject.asObservable();

  dialogState = EditEntityDialogState.EditingForm;
  readonly EditEntityDialogState = EditEntityDialogState;

  closeValue?: ILocationDTO;

  createOrEditLocation(location: ICreateLocationDTO | IUpdateLocationDTO) {
    const currentLocation = this.locationSubject.getValue();

    const error = (err: Error) => { this.notifyFailedSave(); throw err };

    if(currentLocation) {
      this.adminApiService.Locations.updateSingle(currentLocation.id, location).subscribe(
        {
          next: updated => updated ? this.notifySuccessfulSave({ ...currentLocation, ...location }) : this.notifyFailedSave(),
          error,
        }
      );
    } else {
      this.adminApiService.Locations.createSingle(location).subscribe(
        {
          next: created => created ? this.notifySuccessfulSave(created) : this.notifyFailedSave(),
          error,
        }
      );
    }
    this.dialogState = EditEntityDialogState.SavingData;
  }

  notifySuccessfulSave(location: ILocationDTO) {
    this.dialogState = EditEntityDialogState.SaveSuccessful;
    this.closeValue = location;
  }

  notifyFailedSave() {
    this.dialogState = EditEntityDialogState.SaveFailed;
  }
}
