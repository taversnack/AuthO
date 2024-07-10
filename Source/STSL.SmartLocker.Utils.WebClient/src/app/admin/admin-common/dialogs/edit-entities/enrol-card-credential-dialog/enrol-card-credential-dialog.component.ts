import { AfterViewInit, Component, DestroyRef, ElementRef, ViewChild, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { concatWith, debounceTime, fromEvent, take, tap } from 'rxjs';
import { DialogBase } from 'src/app/dialog/directives/dialog-component-base.directive';

enum EnrolState {
  AwaitingCard,
  ReadingCard,
  AcceptedCard,
  FailedToReadCard,
}

export interface IEnrolCardCredentialDialogData  {

}

export interface ICardCredentialCsnHidPair {
  csn: string;
  hid: string;
}

@Component({
  selector: 'admin-enrol-card-credential-dialog',
  templateUrl: './enrol-card-credential-dialog.component.html',
})
export class EnrolCardCredentialDialogComponent extends DialogBase<EnrolCardCredentialDialogComponent, IEnrolCardCredentialDialogData, ICardCredentialCsnHidPair> implements AfterViewInit {

  private readonly destroyRef = inject(DestroyRef);

  private readonly readingDebounceTime = 200;

  @ViewChild('cardInput') cardInput?: ElementRef<HTMLInputElement>;

  dialogState: EnrolState = EnrolState.AwaitingCard;

  closeValue?: ICardCredentialCsnHidPair;

  readonly EnrolState = EnrolState;

  ngAfterViewInit(): void {
    if(this.cardInput) {
      const cardInput$ = fromEvent(this.cardInput.nativeElement, 'input');

      cardInput$.pipe(
        take(1), tap(() => this.dialogState = EnrolState.ReadingCard),
        concatWith(cardInput$.pipe(
            debounceTime(this.readingDebounceTime),
            tap(() => {
              const cardInputValue = this.cardInput?.nativeElement.value ?? '';
              const { csn, hid } = cardInputValue.match(/csn:(?<csn>\d*)mifarehid:(?<hid>\d*)/)?.groups ?? { csn: undefined, hid: undefined };

              if(csn && hid) {
                this.closeValue = { csn, hid };
                this.dialogState = EnrolState.AcceptedCard;
              } else {
                this.dialogState = EnrolState.FailedToReadCard;
              }
            }),
            takeUntilDestroyed(this.destroyRef),
        ))
      ).subscribe()
    }
  }
}
