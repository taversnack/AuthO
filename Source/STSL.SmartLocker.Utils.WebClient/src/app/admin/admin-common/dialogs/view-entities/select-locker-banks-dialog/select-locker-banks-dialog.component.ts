import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatCheckboxChange } from '@angular/material/checkbox';
import { BehaviorSubject, Observable, map } from 'rxjs';
import { DialogBase } from 'src/app/dialog/directives/dialog-component-base.directive';
import { DialogService } from 'src/app/dialog/services/dialog.service';
import { arrayWithRemovedIndex } from 'src/app/shared/lib/utilities';
import { PagingResponse } from 'src/app/shared/models/api';
import { ITableSettings, StickyPosition } from 'src/app/shared/models/data-table';
import { ILocationDTO } from 'src/app/shared/models/location';
import { ILockerBankDTO } from 'src/app/shared/models/locker-bank';
import { IEntityPermissions } from '../../../models/entity-views';

const enum TabIndex {
  Locations = 0,
  LockerBanks = 1,
  Selected = 2
};

export interface ISelectLockerBanksDialogData {
  useAdminRegisteredLocationsAndLockerBanksOnly?: boolean;
  hideLocations?: boolean;
  allowMultipleLockerBanks?: boolean;
  currentlySelectedLocation$?: Observable<ILocationDTO>;
  currentlySelectedLockerBanks$?: Observable<ILockerBankDTO[]>;
  checkboxLabel?: string;
  requireConfirmationDoubleCheck?: boolean;
}

@Component({
  selector: 'admin-select-locker-banks-dialog',
  templateUrl: './select-locker-banks-dialog.component.html',
})
export class SelectLockerBanksDialogComponent extends DialogBase<SelectLockerBanksDialogComponent, ISelectLockerBanksDialogData, ILockerBankDTO[]> implements OnInit {

  private readonly dialogService = inject(DialogService);
  private readonly destroyRef = inject(DestroyRef);

  private readonly currentlySelectedLocationSubject = new BehaviorSubject<ILocationDTO | undefined>(undefined);
  readonly currentlySelectedLocation$ = this.currentlySelectedLocationSubject.asObservable();

  readonly lockerBankPermissions: IEntityPermissions = {};

  selectedTabIndex: TabIndex = TabIndex.Locations;

  private readonly selectedLockerBanksSubject = new BehaviorSubject<ILockerBankDTO[]>([]);
  readonly selectedLockerBanks$ = this.selectedLockerBanksSubject.asObservable().pipe(map(x => new PagingResponse(x ?? [])));

  readonly checkboxLabel = this.data?.checkboxLabel ?? "Select Locker Bank";

  readonly StickyPosition = StickyPosition;

  readonly selectedLockerBanksTableSettings: ITableSettings = {
    a11yLabel: 'Selected Locker Banks',
    disablePaging: true,
  };

  selectedLockerBanksLength = 0;

  ngOnInit(): void {
    this.data?.currentlySelectedLockerBanks$?.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(lockerBanks => {
      this.selectedLockerBanksSubject.next(lockerBanks);
    });

    this.selectedLockerBanks$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(x => {
      this.selectedLockerBanksLength = x.totalRecords ?? 0;
    });
  }

  viewLockerBanksForLocation(location?: ILocationDTO) {
    this.currentlySelectedLocationSubject.next(location);
    this.selectedTabIndex = TabIndex.LockerBanks;
  }

  isLockerBankChecked(lockerBank: ILockerBankDTO): boolean {
    return this.selectedLockerBanksSubject.getValue().some(({ id }) => lockerBank.id === id);
  }

  selectLockerBank(lockerBank: ILockerBankDTO, { checked: isSelected }: MatCheckboxChange) {
    const selectedLocks = this.selectedLockerBanksSubject.getValue();
    const lockAlreadySelectedIndex = selectedLocks.findIndex(({ id }) => lockerBank.id === id);

    if(lockAlreadySelectedIndex >= 0) {
      if(!isSelected) {
        this.selectedLockerBanksSubject.next(arrayWithRemovedIndex(selectedLocks, lockAlreadySelectedIndex));
      }
    } else if(isSelected) {
      const currentSelection = this.data?.allowMultipleLockerBanks ? selectedLocks : [];
      this.selectedLockerBanksSubject.next([...currentSelection, lockerBank]);
    }
  }

  confirmLockerBankSelection() {
    const selectedLockerBanks = this.selectedLockerBanksSubject.getValue();
    if(this.data?.requireConfirmationDoubleCheck) {
      this.dialogService.confirm(['Adding the following locker banks:', ...selectedLockerBanks.map(x => x.name)])
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe(confirmed => confirmed && this.close(selectedLockerBanks));
    } else {
      this.close(selectedLockerBanks);
    }
  }

  resetLockerBankSelection() {
    this.selectedLockerBanksSubject.next([]);
  }
}
