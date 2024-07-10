import { Component, DestroyRef, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, UntypedFormGroup, Validators } from '@angular/forms';
import { Observable, filter, of } from 'rxjs';
import { DialogService } from 'src/app/dialog/services/dialog.service';
import { ITextInputSettings, textControlFromSettings } from 'src/app/shared/components/text-input/text-input.component';
import { DateTime } from 'src/app/shared/models/common';
import { ICreateLockerDTO, ILockerDTO, IUpdateLockerDTO, LockerSecurityType } from 'src/app/shared/models/locker';

@Component({
  selector: 'admin-edit-locker-form',
  templateUrl: './edit-locker-form.component.html',
})
export class EditLockerFormComponent implements OnInit {

  private readonly dialogService = inject(DialogService);
  private readonly destroyRef = inject(DestroyRef);

  @Input() locker$: Observable<ILockerDTO | undefined> = of();
  @Input() allowSmartLocksOnly = false;

  @Output() lockerSubmitted = new EventEmitter<ICreateLockerDTO | IUpdateLockerDTO>();

  readonly LockerSecurityType = LockerSecurityType;

  hasLocker = false;
  showLeaseExpiryDatePicker = false;

  readonly lockerLabelSettings: ITextInputSettings = {
    name: 'label',
    required: true,
    maxLength: 256,
  };

  readonly lockerServiceTagSettings: ITextInputSettings = {
    name: 'serviceTag',
    required: true,
    maxLength: 32,
  };

  lockerAbsoluteLeaseExpiryFormGroup = new FormGroup({
    'date': new FormControl<DateTime | null>(null),
    'time': new FormControl<string | null>(null),
  });

  lockerSecurityTypeControl = new FormControl(LockerSecurityType.SmartLock, Validators.required);

  lockerForm = new UntypedFormGroup({
    'securityType': this.lockerSecurityTypeControl,
    // 'absoluteLeaseExpiry': this.lockerAbsoluteLeaseExpiryFormGroup
  });

  lockerLabelControl = textControlFromSettings(this.lockerLabelSettings, this.lockerForm);
  lockerServiceTagControl = textControlFromSettings(this.lockerServiceTagSettings, this.lockerForm);

  ngOnInit(): void {
    this.locker$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(locker => {
      // TODO: enable / disable lease expiry controls
      if(locker) {
        if(locker.absoluteLeaseExpiry) {
          this.enableAbsoluteLeaseExpiry();
          const absoluteLeaseExpiry = this.deserializeAbsoluteLeaseExpiry(locker.absoluteLeaseExpiry)
          this.lockerForm.patchValue({ ...locker, absoluteLeaseExpiry })
        } else {
          this.disableAbsoluteLeaseExpiry();
          this.lockerForm.patchValue(locker);
        }

        this.hasLocker = true;

        this.setServiceTagRequirementBasedOnSecurityType(locker.securityType);
      } else {
        this.disableAbsoluteLeaseExpiry();
        this.hasLocker = false;
        this.lockerForm.patchValue({ label: '', serviceTag: '', securityType: LockerSecurityType.SmartLock });
        this.lockerForm.markAsUntouched();
      }
    });

    this.lockerSecurityTypeControl.valueChanges.pipe(filter((x): x is LockerSecurityType => x !== null), takeUntilDestroyed(this.destroyRef)).subscribe(x => {
      this.setServiceTagRequirementBasedOnSecurityType(x);
    });
  }

  private setServiceTagRequirementBasedOnSecurityType(securityType: LockerSecurityType) {
    if(securityType === LockerSecurityType.SmartLock) {
      this.lockerServiceTagControl.addValidators(Validators.required);
    } else {
      this.lockerServiceTagControl.removeValidators(Validators.required);
    }
    this.lockerServiceTagControl.updateValueAndValidity();
  }

  submitForm() {
    if(this.lockerForm.invalid) {
      this.dialogService.alert("Ensure all required properties are filled in and valid", "Validation Error");
      return;
    }

    const locker = this.lockerForm.value;
    const absoluteLeaseExpiry = this.serializeAbsoluteLeaseExpiry();
    if(!locker.serviceTag) {
      delete locker.serviceTag;
    }

    this.lockerSubmitted.emit({ ...locker, absoluteLeaseExpiry } as ICreateLockerDTO | IUpdateLockerDTO);
  }

  private enableAbsoluteLeaseExpiry() {
    this.lockerAbsoluteLeaseExpiryFormGroup.enable();
    this.showLeaseExpiryDatePicker = true;
  }

  private disableAbsoluteLeaseExpiry() {
    this.lockerAbsoluteLeaseExpiryFormGroup.disable();
    this.showLeaseExpiryDatePicker = false;
  }

  // TODO: implement (de)serialization behaviours
  private deserializeAbsoluteLeaseExpiry(absoluteLeaseExpiry: DateTime): { date: DateTime, time: string } {
    return { date: '', time: '' };
  }

  private serializeAbsoluteLeaseExpiry(): DateTime | undefined {
    return undefined;
  }

  // date: FormControl<moment.Moment | null> = new FormControl(moment(this.data?.entity?.absoluteLeaseExpiry));
  // time: FormControl<string | null> = new FormControl(`${this.data?.entity?.absoluteLeaseExpiry?.getUTCHours().toString().padStart(2, '0')}:${this.data?.entity?.absoluteLeaseExpiry?.getUTCMinutes().toString().padStart(2, '0')}`);

  // parseDateTime(date: moment.Moment, time: moment.Moment): moment.Moment {
  //   // if(!this.date.value || !this.time.value) {
  //   //   throw new Error('invalid value for date or time');
  //   // }
  //   // const time = moment(this.time.value, 'HH:mm');
  //   // const dateTime = moment(this.date.value);
  //   const dateTime = moment(date);
  //   dateTime.set({
  //     hour: time.get('hour'),
  //     minute: time.get('minute'),
  //     second: 0,
  //     millisecond: 0
  //   });
  //   return dateTime;
  // }
}
