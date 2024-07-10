import { Component, DestroyRef, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { UntypedFormGroup } from '@angular/forms';
import { Observable, of } from 'rxjs';
import { AuthService } from 'src/app/auth/services/auth.service';
import { DialogService } from 'src/app/dialog/services/dialog.service';
import { ITextInputSettings, textControlFromSettings } from 'src/app/shared/components/text-input/text-input.component';
import { ICardHolderDTO, ICreateCardHolderDTO, IUpdateCardHolderDTO } from 'src/app/shared/models/card-holder';

@Component({
  selector: 'admin-edit-card-holder-form',
  templateUrl: './edit-card-holder-form.component.html',
})
export class EditCardHolderFormComponent implements OnInit {

  private readonly dialogService = inject(DialogService);
  private readonly authService = inject(AuthService);
  private readonly destroyRef = inject(DestroyRef);

  @Input() cardHolder$: Observable<ICardHolderDTO | undefined> = of();

  @Output() cardHolderSubmitted = new EventEmitter<ICreateCardHolderDTO | IUpdateCardHolderDTO>();

  cardHolderAliasSingular$ = this.authService.cardHolderAliasSingular$;
  cardHolderUniqueIdentifierAlias = this.authService.cardHolderUniqueIdentifierAlias;

  hasCardHolder = false;

  readonly cardHolderFirstNameSettings: ITextInputSettings = {
    name: 'firstName',
    required: true,
    maxLength: 256,
  };

  readonly cardHolderLastNameSettings: ITextInputSettings = {
    name: 'lastName',
    required: true,
    maxLength: 256,
  };

  readonly cardHolderUniqueIdentifierSettings: ITextInputSettings = {
    name: 'uniqueIdentifier',
    required: true,
    maxLength: 256,
    typeOverride: 'numeric',
    display: this.cardHolderUniqueIdentifierAlias,
  };

  readonly cardHolderEmailSettings: ITextInputSettings = {
    name: 'email',
    required: false,
    maxLength: 256,
    typeOverride: 'email',
  };

  cardHolderForm = new UntypedFormGroup({});

  cardHolderFirstNameControl = textControlFromSettings(this.cardHolderFirstNameSettings, this.cardHolderForm);
  cardHolderLastNameControl = textControlFromSettings(this.cardHolderLastNameSettings, this.cardHolderForm);
  cardHolderUniqueIdentifierControl = textControlFromSettings(this.cardHolderUniqueIdentifierSettings, this.cardHolderForm);
  cardHolderEmailControl = textControlFromSettings(this.cardHolderEmailSettings, this.cardHolderForm);

  ngOnInit(): void {
    this.cardHolder$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(cardHolder => {
      if(cardHolder) {

        this.cardHolderForm.patchValue(cardHolder);
        
        this.hasCardHolder = true;
      } else {
        this.hasCardHolder = false;
      }
    });
  }

  submitForm() {
    if(this.cardHolderForm.invalid) {
      this.dialogService.alert("Ensure all required properties are filled in and valid", "Validation Error");
      return;
    }

    const cardHolder = this.cardHolderForm.value as ICreateCardHolderDTO | IUpdateCardHolderDTO;
    if(!cardHolder.email) {
      delete cardHolder.email;
    }

    this.cardHolderSubmitted.emit(cardHolder);
  }

}
