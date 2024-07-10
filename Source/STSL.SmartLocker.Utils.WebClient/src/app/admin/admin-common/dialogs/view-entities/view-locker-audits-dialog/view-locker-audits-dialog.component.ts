import { Component } from '@angular/core';
import { of } from 'rxjs';
import { DialogBase } from 'src/app/dialog/directives/dialog-component-base.directive';
import { ILockerDTO } from 'src/app/shared/models/locker';
import { ILockerStatusDTO } from 'src/app/shared/models/locker-status';

export interface IViewLockerAuditsDialogData {
  currentlySelectedLocker: ILockerDTO | ILockerStatusDTO;
}

@Component({
  selector: 'admin-view-locker-audits-dialog',
  templateUrl: './view-locker-audits-dialog.component.html',
})
export class ViewLockerAuditsDialogComponent extends DialogBase<ViewLockerAuditsDialogComponent, IViewLockerAuditsDialogData> {
  currentlySelectedLocker$ = of(this.data?.currentlySelectedLocker);
}
