import { Component, DestroyRef, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, UntypedFormGroup, Validators } from '@angular/forms';
import { Observable, of } from 'rxjs';
import { AdminDialogService } from 'src/app/admin/services/admin-dialog.service';
import { ITextInputSettings, textControlFromSettings } from 'src/app/shared/components/text-input/text-input.component';
import { decimalCsnToLockFormat, lockFormatCsnToDecimal } from 'src/app/shared/lib/utilities';
import { CardType, ICardCredentialDTO, ICreateCardCredentialDTO, IUpdateCardCredentialDTO } from 'src/app/shared/models/card-credential';

@Component({
  selector: 'admin-edit-card-credential-form',
  templateUrl: './edit-card-credential-form.component.html',
})
export class EditCardCredentialFormComponent implements OnInit {

  private readonly adminDialogService = inject(AdminDialogService);
  private readonly destroyRef = inject(DestroyRef);

  @Input() cardCredential$: Observable<ICardCredentialDTO | undefined> = of();

  @Output() cardCredentialSubmitted = new EventEmitter<ICreateCardCredentialDTO | IUpdateCardCredentialDTO>();

  hasCardCredential = false;

  cardCredentialSerialNumberControl = new FormControl('', [Validators.maxLength(32), Validators.pattern(/^\d*?$/)]);
  cardCredentialHidNumberControl = new FormControl('', [Validators.required, Validators.maxLength(32), Validators.pattern(/^\d*?$/)]);
  cardCredentialCardTypeControl = new FormControl(CardType.User, Validators.required);

  cardCredentialForm = new UntypedFormGroup({
    'serialNumber': this.cardCredentialSerialNumberControl,
    'hidNumber': this.cardCredentialHidNumberControl,
    'cardType': this.cardCredentialCardTypeControl,
  });

  cardCredentialLabelSettings: ITextInputSettings = {
    name: 'cardLabel',
    maxLength: 256,
  };

  cardCredentialLabelControl = textControlFromSettings(this.cardCredentialLabelSettings, this.cardCredentialForm);

  readonly CardType = CardType;

  ngOnInit(): void {
    this.cardCredential$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(cardCredential => {
      this.hasCardCredential = cardCredential !== undefined;

      if(cardCredential) {
        const { serialNumber: csnHex, ...cardCredentialProperties } = cardCredential;

        const serialNumber = csnHex ? lockFormatCsnToDecimal(csnHex) : undefined;

        this.cardCredentialForm.patchValue({ serialNumber, ...cardCredentialProperties });
      }
    });
  }

  submitForm() {
    if(this.cardCredentialForm.invalid) {
      this.adminDialogService.alert("Ensure all required properties are filled in and valid", "Validation Error");
      return;
    }

    const cardCredential = this.cardCredentialForm.value as ICreateCardCredentialDTO | IUpdateCardCredentialDTO;

    const { serialNumber: csnDecimal, ...cardCredentialProperties } = cardCredential;
    const serialNumber = csnDecimal ? decimalCsnToLockFormat(csnDecimal) : undefined;

    this.cardCredentialSubmitted.emit({ serialNumber, ...cardCredentialProperties });
  }

  enrolCardCredential() {
    this.adminDialogService.openEnrolCardCredentialDialog().subscribe(({ csn: serialNumber, hid: hidNumber }) => this.cardCredentialForm.patchValue({ serialNumber, hidNumber }));
  }
}
