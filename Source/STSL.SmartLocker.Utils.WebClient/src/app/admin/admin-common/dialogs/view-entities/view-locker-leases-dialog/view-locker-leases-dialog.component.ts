import { Component, inject } from '@angular/core';
import { AdminApiService } from 'src/app/admin/services/admin-api.service';
import { DialogBase } from 'src/app/dialog/directives/dialog-component-base.directive';
import { PageFilterSortRequest } from 'src/app/shared/models/api';
import { ILockerLeaseDTO } from 'src/app/shared/models/locker-lease';

export interface IViewLockerLeasesDialogData {
  useEndpoint?: PageFilterSortRequest<ILockerLeaseDTO>;
  hideCardCredentialColumns?: boolean;
  hideCardHolderColumns?: boolean;
  hideLockerColumns?: boolean;
  hideLockColumns?: boolean;
  hideEndedByMasterCardColumn?: boolean;
}

@Component({
  selector: 'admin-view-locker-leases-dialog',
  templateUrl: './view-locker-leases-dialog.component.html',
})
export class ViewLockerLeasesDialogComponent extends DialogBase<ViewLockerLeasesDialogComponent, IViewLockerLeasesDialogData> {

  private readonly adminApiService = inject(AdminApiService);

  readonly settings: Required<IViewLockerLeasesDialogData> = {
    useEndpoint: this.adminApiService.LockerLeases.getMany,
    hideCardCredentialColumns: false,
    hideCardHolderColumns: false,
    hideLockerColumns: false,
    hideLockColumns: false,
    hideEndedByMasterCardColumn: true,
    ...this.data
  };

}
