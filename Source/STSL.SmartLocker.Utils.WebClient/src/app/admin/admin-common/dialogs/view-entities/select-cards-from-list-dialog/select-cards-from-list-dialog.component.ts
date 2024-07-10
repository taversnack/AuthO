import { AfterViewInit, ChangeDetectionStrategy, ChangeDetectorRef, Component, TemplateRef, ViewChild, inject } from '@angular/core';
import { BehaviorSubject, of } from 'rxjs';
import { DialogBase } from 'src/app/dialog/directives/dialog-component-base.directive';
import { arrayWithRemovedIndex, checkEntityIdEquality, lockFormatCsnToDecimal } from 'src/app/shared/lib/utilities';
import { PagingResponse } from 'src/app/shared/models/api';
import { ICardCredentialDTO } from 'src/app/shared/models/card-credential';
import { ITableSettings, StickyPosition, TableColumn, defaultTableSettings } from 'src/app/shared/models/data-table';

export interface ISelectCardsFromListDialogData {
  cardCredentials: ICardCredentialDTO[];
  currentlySelectedCards?: ICardCredentialDTO[];
  showCardTypeColumn?: boolean;
  selectionLimit?: number | null;
  ensureCardSelectionsHaveSerialNumber?: boolean;
}

@Component({
  selector: 'admin-select-cards-from-list-dialog',
  templateUrl: './select-cards-from-list-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SelectCardsFromListDialogComponent extends DialogBase<SelectCardsFromListDialogComponent, ISelectCardsFromListDialogData, ICardCredentialDTO[]> implements AfterViewInit {

  private readonly changeDetectorRef = inject(ChangeDetectorRef);

  @ViewChild('actions') actions?: TemplateRef<ICardCredentialDTO>;

  readonly settings: Required<ISelectCardsFromListDialogData> = {
    cardCredentials: [],
    currentlySelectedCards: [],
    showCardTypeColumn: false,
    selectionLimit: 1,
    ensureCardSelectionsHaveSerialNumber: true,
    ...this.data
  };

  readonly cardCredentials$ = of(new PagingResponse([...this.data?.cardCredentials ?? []]));

  private readonly cardCredentialColumnsSubject = new BehaviorSubject<TableColumn<ICardCredentialDTO>[]>([]);
  readonly cardCredentialColumns$ = this.cardCredentialColumnsSubject.asObservable();

  readonly tableSettings: ITableSettings = { ...defaultTableSettings, disablePaging: true };

  selectedCards: ICardCredentialDTO[] = [...this.settings.currentlySelectedCards];

  ngAfterViewInit(): void {
    const cardCredentialColumns: TableColumn<ICardCredentialDTO>[] = [
      { name: 'serialNumber', headingDisplay: 'CSN', getValue: x => x.serialNumber ? lockFormatCsnToDecimal(x.serialNumber) : undefined },
      { name: 'hidNumber', headingDisplay: 'HID' },
      'cardLabel',
    ];

    if(this.settings.showCardTypeColumn) {
      cardCredentialColumns.push('cardType');
    }

    if(this.actions) {
      cardCredentialColumns.push({ name: 'useCard?', template: this.actions, stickyPosition: StickyPosition.End });
    }

    this.cardCredentialColumnsSubject.next(cardCredentialColumns);
    this.changeDetectorRef.detectChanges();
  }

  toggleSelectedCard(cardCredential: ICardCredentialDTO, isSelected: boolean) {
    const cardAlreadySelectedIndex = this.selectedCards.findIndex(({ id }) => checkEntityIdEquality(id, cardCredential.id));

    if(cardAlreadySelectedIndex >= 0) {
      if(!isSelected) {
        this.selectedCards = arrayWithRemovedIndex(this.selectedCards, cardAlreadySelectedIndex);
      }
    } else {
      if(isSelected) {
        this.selectedCards.push(cardCredential);
      }
    }
    // this.changeDetectorRef.detectChanges();
  }

  confirmSelection() {
    this.close(this.selectedCards);
  }

  resetSelection() {
    this.selectedCards = [];
    // this.changeDetectorRef.detectChanges();
  }

  isCardSelected(cardCredential: ICardCredentialDTO): boolean {
    return this.selectedCards.some(({ id }) => checkEntityIdEquality(id, cardCredential.id));
  }
}
