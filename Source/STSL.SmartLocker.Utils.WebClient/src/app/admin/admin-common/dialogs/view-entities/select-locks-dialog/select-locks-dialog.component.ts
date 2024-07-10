import { ChangeDetectorRef, Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatCheckboxChange } from '@angular/material/checkbox';
import { BehaviorSubject, Observable, map, of, tap } from 'rxjs';
import { DialogBase } from 'src/app/dialog/directives/dialog-component-base.directive';
import { DialogService } from 'src/app/dialog/services/dialog.service';
import { arrayWithRemovedIndex } from 'src/app/shared/lib/utilities';
import { PagingResponse } from 'src/app/shared/models/api';
import { StickyPosition, TableColumn, defaultTableSettings } from 'src/app/shared/models/data-table';
import { ILockDTO } from 'src/app/shared/models/lock';

export interface ISelectLocksDialogData {
  currentlySelectedLocks$?: Observable<ILockDTO[]>;
  isLoadingCurrentlySelectedLocks$?: Observable<boolean>;
  allowMultipleLocks?: boolean;
  showLockerIdColumn?: boolean;
  warnIfSelectedLocksHaveLockerIds?: boolean;
  allowSelectionOfLocksWithLockerIds?: boolean;
  requireConfirmationDoubleCheck?: boolean;
  // hideSelectedTab: boolean;
}

const defaultSelectLocksDialogData: Required<ISelectLocksDialogData> = {
  currentlySelectedLocks$: of([]),
  isLoadingCurrentlySelectedLocks$: of(false),
  allowMultipleLocks: false,
  showLockerIdColumn: true,
  warnIfSelectedLocksHaveLockerIds: true,
  allowSelectionOfLocksWithLockerIds: true,
  requireConfirmationDoubleCheck: true,
  // hideSelectedTab: false,
}

const lockColumns: TableColumn<ILockDTO>[] = [
  'serialNumber',
  'firmwareVersion',
  'operatingMode',
  { name: 'lockerId', sortable: false },
]

const enum TabIndex {
  Locks = 0,
  SelectedLocks = 1,
};

@Component({
  selector: 'admin-select-locks-dialog',
  templateUrl: './select-locks-dialog.component.html',
})
export class SelectLocksDialogComponent extends DialogBase<SelectLocksDialogComponent, ISelectLocksDialogData, ILockDTO[]> implements OnInit {

  private readonly dialogService = inject(DialogService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly changeDetector = inject(ChangeDetectorRef);

  readonly settings: Required<ISelectLocksDialogData> = { ...this.data, ...defaultSelectLocksDialogData };

  private readonly selectedLocksSubject = new BehaviorSubject<ILockDTO[]>([]);
  readonly selectedLocks$ = this.selectedLocksSubject.asObservable().pipe(tap(locks => this.selectedLocksLength = locks.length), map(locks => new PagingResponse(locks)));

  selectedLocksLength = 0;

  selectedTabIndex: TabIndex = TabIndex.Locks;

  readonly StickyPosition = StickyPosition;

  readonly selectedLocksTableSettings = { ...defaultTableSettings, disablePaging: true };

  ngOnInit(): void {
    this.data?.currentlySelectedLocks$?.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(locks => {
      this.selectedLocksSubject.next(locks);
      this.changeDetector.detectChanges();
    });
  }

  useLock(lock: ILockDTO, { checked: isSelected }: MatCheckboxChange) {
    const selectedLocks = this.selectedLocksSubject.getValue();
    const lockAlreadySelectedIndex = selectedLocks.findIndex(({ id }) => lock.id === id);

    if(lockAlreadySelectedIndex >= 0) {
      if(!isSelected) {
        this.selectedLocksSubject.next(arrayWithRemovedIndex(selectedLocks, lockAlreadySelectedIndex));
      }
    } else if(isSelected) {
      const currentSelection = this.settings.allowMultipleLocks ? selectedLocks : [];
      this.selectedLocksSubject.next([...currentSelection, lock]);
    }
  }

  resetLockSelection() {
    this.selectedLocksSubject.next([]);
  }

  isLockChecked(lock: ILockDTO): boolean {
    const selectedLocks = this.selectedLocksSubject.getValue();
    return selectedLocks.some(({ id }) => lock.id === id);
  }

  confirmLockSelection() {
    const selectedLocks = this.selectedLocksSubject.getValue();

    let lockAlreadyHasLockerId = false;

    const selectedLocksMessage = selectedLocks.map(({ serialNumber, lockerId }) => {
      if(lockerId) {
        lockAlreadyHasLockerId = true;
      }
      return serialNumber;
    }).join(', ');

    let confirmationMessage = '';
    let title = '';

    if(this.settings.warnIfSelectedLocksHaveLockerIds && lockAlreadyHasLockerId) {
      confirmationMessage = `A selected lock already has a locker assigned, do you still wish to confirm the following locks: ${selectedLocksMessage}?`;
      title = 'Warning! Lock has locker assigned already';
    } else {
      confirmationMessage = `Confirm the following locks: ${selectedLocksMessage}?`;
      title = 'Confirm locks';
    }

    if(this.settings.warnIfSelectedLocksHaveLockerIds || this.settings.requireConfirmationDoubleCheck) {
      this.dialogService.confirm(confirmationMessage, title).subscribe(confirmed => {
        if(confirmed) {
          this.close(selectedLocks);
        }
      });
    } else {
      this.close(selectedLocks);
    }
  }
}
