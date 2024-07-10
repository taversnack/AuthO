import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatCheckboxChange } from '@angular/material/checkbox';
import { BehaviorSubject, Observable } from 'rxjs';
import { DialogBase } from 'src/app/dialog/directives/dialog-component-base.directive';
import { DialogService } from 'src/app/dialog/services/dialog.service';
import { arrayWithRemovedIndex } from 'src/app/shared/lib/utilities';
import { ILocationDTO } from 'src/app/shared/models/location';
import { ILockerDTO, LockerSecurityType } from 'src/app/shared/models/locker';
import { ILockerAndLockDTO } from 'src/app/shared/models/locker-and-lock';
import { ILockerBankDTO } from 'src/app/shared/models/locker-bank';

const enum TabIndex {
  Locations = 0,
  LockerBanks = 1,
  Lockers = 2
};

export interface ISelectLockersDialogData {
  useAdminRegisteredLocationsAndLockerBanksOnly?: boolean;
  hideLocations?: boolean;
  hideLockerBanks?: boolean;
  allowMultipleLockers?: boolean;
  currentlySelectedLocation$?: Observable<ILocationDTO>;
  currentlySelectedLockerBank$?: Observable<ILockerBankDTO>;
  currentlySelectedLockers$?: Observable<ILockerDTO[]>;
  allowSelectingNonSmartLockers?: boolean;
  checkboxLabel?: string;
  requireConfirmationDoubleCheck?: boolean;
}

@Component({
  selector: 'admin-select-lockers-dialog',
  templateUrl: './select-lockers-dialog.component.html',
})
export class SelectLockersDialogComponent extends DialogBase<SelectLockersDialogComponent, ISelectLockersDialogData, ILockerDTO[]> implements OnInit {

  private readonly dialogService = inject(DialogService);
  private readonly destroyRef = inject(DestroyRef);

  private readonly currentlySelectedLocationSubject = new BehaviorSubject<ILocationDTO | undefined>(undefined);
  readonly currentlySelectedLocation$ = this.currentlySelectedLocationSubject.asObservable();

  private readonly currentlySelectedLockerBankSubject = new BehaviorSubject<ILockerBankDTO | undefined>(undefined);
  readonly currentlySelectedLockerBank$ = this.currentlySelectedLockerBankSubject.asObservable();

  selectedTabIndex: TabIndex = TabIndex.Locations;

  selectedLockers: ILockerDTO[] = [];

  readonly LockerSecurityType = LockerSecurityType;

  readonly checkboxLabel = this.data?.checkboxLabel ?? "Select Locker";

  ngOnInit(): void {
    this.data?.currentlySelectedLocation$?.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(location => {
      this.currentlySelectedLocationSubject.next(location);
    });

    this.data?.currentlySelectedLockerBank$?.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(lockerBank => {
      this.currentlySelectedLockerBankSubject.next(lockerBank);
    });

    this.data?.currentlySelectedLockers$?.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(lockers => {
      this.selectedLockers = lockers;
    });
  }

  viewLockerBanksForLocation(location?: ILocationDTO) {
    this.currentlySelectedLocationSubject.next(location);
    this.selectedTabIndex = TabIndex.LockerBanks;
  }

  viewLockersForLockerBank(lockerBank?: ILockerBankDTO) {
    this.currentlySelectedLockerBankSubject.next(lockerBank);
    this.selectedTabIndex = TabIndex.Lockers;
  }

  isLockerChecked(lockerAndLock: ILockerAndLockDTO): boolean {
    const { locker } = lockerAndLock;
    return this.selectedLockers.some(({ id }) => locker.id === id);
  }

  selectLocker(lockerAndLock: ILockerAndLockDTO, { checked: isSelected }: MatCheckboxChange) {
    const { locker } = lockerAndLock;
    const lockerAlreadySelectedIndex = this.selectedLockers.findIndex(({ id }) => locker.id === id);

    if(lockerAlreadySelectedIndex >= 0) {
      if(!isSelected) {
        this.selectedLockers = arrayWithRemovedIndex(this.selectedLockers, lockerAlreadySelectedIndex);
      }
    } else if(isSelected) {
      const currentSelection = this.data?.allowMultipleLockers ? this.selectedLockers : [];
      this.selectedLockers = [...currentSelection, locker];
    }
  }

  confirmLockerSelection() {
    if(this.data?.requireConfirmationDoubleCheck) {
      const selectedLockersMessage = this.selectedLockers.map(({ label, serviceTag }) => `${label} ${serviceTag ? '(' + serviceTag + ')' : ''}`).join(', ');
      this.dialogService.confirm(`Adding the following lockers: ${selectedLockersMessage}`).subscribe(confirmed => {
        if(confirmed) {
          this.close(this.selectedLockers);
        }
      });
    } else {
      this.close(this.selectedLockers);
    }
  }

  resetLockerSelection() {
    this.selectedLockers = [];
  }
}
