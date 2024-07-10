import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatCheckboxChange } from '@angular/material/checkbox';
import { BehaviorSubject, Observable, map, of } from 'rxjs';
import { AuthService } from 'src/app/auth/services/auth.service';
import { DialogBase } from 'src/app/dialog/directives/dialog-component-base.directive';
import { arrayWithRemovedIndex } from 'src/app/shared/lib/utilities';
import { PagingResponse } from 'src/app/shared/models/api';
import { ICardHolderDTO, defaultCardHolderAliasPlural, defaultCardHolderAliasSingular } from 'src/app/shared/models/card-holder';
import { StickyPosition, defaultTableSettings } from 'src/app/shared/models/data-table';
import { IEntityPermissions } from '../../../models/entity-views';

export interface ISelectCardHoldersDialogData {
  cardHolderPermissions?: IEntityPermissions;
  allowMultipleCardHolders?: boolean;
  currentlySelectedCardHolders?: Observable<ICardHolderDTO[]>;
  requireCardHolderEmail?: boolean;
}

const defaultSelectCardHoldersDialogData: Required<ISelectCardHoldersDialogData> = {
  cardHolderPermissions: {},
  allowMultipleCardHolders: false,
  currentlySelectedCardHolders: of([]),
  requireCardHolderEmail: false,
};

const enum TabIndex {
  CardHolders = 0,
  SelectedCardHolders = 1,
};

@Component({
  selector: 'admin-select-card-holders-dialog',
  templateUrl: './select-card-holders-dialog.component.html',
})
export class SelectCardHoldersDialogComponent extends DialogBase<SelectCardHoldersDialogComponent, ISelectCardHoldersDialogData, ICardHolderDTO[]> implements OnInit {

  private readonly authService = inject(AuthService);
  private readonly destroyRef = inject(DestroyRef);

  cardHolderAliasSingular = this.authService.cardHolderAliasSingular;
  cardHolderAliasPlural = this.authService.cardHolderAliasPlural;
  cardHolderUniqueIdentifierAlias = this.authService.cardHolderUniqueIdentifierAlias;

  readonly settings: Required<ISelectCardHoldersDialogData> = { ...defaultSelectCardHoldersDialogData, ...this.data  };

  private readonly selectedCardHoldersSubject = new BehaviorSubject<ICardHolderDTO[]>([]);
  selectedCardHolders$ = this.selectedCardHoldersSubject.pipe(map(cardHolders => new PagingResponse(cardHolders)));

  readonly selectedCardHolderTableSettings = { ...defaultTableSettings, disablePaging: true };

  selectedCardHoldersLength = 0;

  selectedTabIndex = TabIndex.CardHolders;

  isLoadingSelectedCardHolders: boolean = false;

  readonly StickyPosition = StickyPosition;

  ngOnInit(): void {
    this.authService.currentTenantDetails$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(x => {
      this.cardHolderAliasSingular = x?.cardHolderAliasSingular ?? defaultCardHolderAliasSingular;
      this.cardHolderAliasPlural = x?.cardHolderAliasPlural ?? defaultCardHolderAliasPlural;
    })

    this.settings.currentlySelectedCardHolders.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(cardHolders => {
      this.isLoadingSelectedCardHolders = false;
      this.selectedCardHoldersSubject.next(cardHolders);
    });

    this.selectedCardHoldersSubject.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(cardHolders => this.selectedCardHoldersLength = cardHolders.length);
  }

  useCardHolder(cardHolder: ICardHolderDTO, event: MatCheckboxChange) {

    const isSelected = event.checked;

    const selectedCardHolders = this.selectedCardHoldersSubject.getValue();

    const selectedCardHolderIndex = selectedCardHolders.findIndex(({ id }) => cardHolder.id === id);

    if(isSelected) {
      if(selectedCardHolderIndex < 0) {
        (this.settings.allowMultipleCardHolders || !selectedCardHolders.length) ?
        this.selectedCardHoldersSubject.next([ ...selectedCardHolders, cardHolder ])
        :
        this.selectedCardHoldersSubject.next([cardHolder]);
      }
    } else {
      if(selectedCardHolderIndex >= 0) {
        this.selectedCardHoldersSubject.next(arrayWithRemovedIndex(selectedCardHolders, selectedCardHolderIndex));
      }
    }
  }

  confirmCardHolderSelection() {
    const selectedCardHolders = this.selectedCardHoldersSubject.getValue();

    this.close(selectedCardHolders);
  }

  resetCardHolderSelection() {
    this.selectedCardHoldersSubject.next([]);
  }

  isCardHolderChecked(cardHolder: ICardHolderDTO): boolean {
    const selectedCardHolders = this.selectedCardHoldersSubject.getValue();

    return selectedCardHolders.some(({ id }) => cardHolder.id === id);
  }
}
