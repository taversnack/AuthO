import { Component, DestroyRef, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, UntypedFormGroup, Validators } from '@angular/forms';
import { Observable, Subscription, distinctUntilChanged, of } from 'rxjs';
import { DialogService } from 'src/app/dialog/services/dialog.service';
import { FeatureFlag } from 'src/app/feature-flag/feature-flag.type';
import { FeatureFlagService } from 'src/app/feature-flag/services/feature-flag.service';
import { textControlFromSettings } from 'src/app/shared/components/text-input/text-input.component';
import { ICreateLockerBankDTO, ILockerBankDTO, IUpdateLockerBankDTO, LockerBankBehaviour } from 'src/app/shared/models/locker-bank';

@Component({
  selector: 'admin-edit-locker-bank-form',
  templateUrl: './edit-locker-bank-form.component.html',
})
export class EditLockerBankFormComponent implements OnInit {

  private readonly dialogService = inject(DialogService);
  private readonly featureFlagService = inject(FeatureFlagService);
  private readonly destroyRef = inject(DestroyRef);

  @Input() lockerBank$: Observable<ILockerBankDTO | undefined> = of();

  @Output() lockerBankSubmitted = new EventEmitter<ICreateLockerBankDTO | IUpdateLockerBankDTO>();

  lockerBankSubscription?: Subscription;
  behaviourSubscription?: Subscription;

  hasLockerBank = false;

  disableTemporaryLockerBanks = this.featureFlagService.hasFeatureFlagSet(FeatureFlag.DisableShiftLockerBanks);

  readonly lockerBankNameSettings = {
    name: 'name',
    required: true,
    maxLength: 256,
  };

  readonly lockerBankDescriptionSettings = {
    name: 'description',
    maxLength: 256,
  };

  LockerBankBehaviour = LockerBankBehaviour;

  private readonly defaultLeaseDurationValidators = [Validators.required, Validators.pattern(/^[0-9]{2}:[0-9]{2}:[0-9]{2}$/)];

  lockerBankBehaviourControl = new FormControl(LockerBankBehaviour.Permanent, Validators.required);
  lockerBankDefaultLeaseDurationControl = new FormControl();

  lockerBankForm = new UntypedFormGroup({
    'behaviour': this.lockerBankBehaviourControl,
    'defaultLeaseDuration': this.lockerBankDefaultLeaseDurationControl,
  });

  lockerBankNameControl = textControlFromSettings(this.lockerBankNameSettings, this.lockerBankForm);
  lockerBankDescriptionControl = textControlFromSettings(this.lockerBankDescriptionSettings, this.lockerBankForm);

  ngOnInit(): void {
    this.lockerBankBehaviourControl.valueChanges.pipe(distinctUntilChanged(), takeUntilDestroyed(this.destroyRef)).subscribe(behaviour => {
      const validators = behaviour === LockerBankBehaviour.Temporary ? this.defaultLeaseDurationValidators : [];
      this.lockerBankDefaultLeaseDurationControl.setValidators(validators);
      this.lockerBankDefaultLeaseDurationControl.updateValueAndValidity();
    });

    this.lockerBank$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(lockerBank => {
      this.hasLockerBank = lockerBank !== undefined;
      this.lockerBankForm.patchValue(lockerBank ? lockerBank : { name: '', description: '', behaviour: LockerBankBehaviour.Permanent });
    });

    this.featureFlagService.observeFeatureFlagChanges(FeatureFlag.DisableShiftLockerBanks).pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(flagIsSet => this.disableTemporaryLockerBanks = flagIsSet);
  }

  submitForm() {
    if (this.lockerBankForm.invalid) {
      this.dialogService.error("Ensure all required properties are filled in and valid", "Validation Error");
      return;
    }

    const lockerBank = this.lockerBankForm.value;
    this.lockerBankSubmitted.emit(lockerBank as ICreateLockerBankDTO | IUpdateLockerBankDTO);
  }
}
