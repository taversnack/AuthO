import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { Component, DestroyRef, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, UntypedFormGroup, Validators } from '@angular/forms';
import * as moment from 'moment';
import { Observable, map, of } from 'rxjs';
import { DialogService } from 'src/app/dialog/services/dialog.service';
import { ITextInputSettings, textControlFromSettings } from 'src/app/shared/components/text-input/text-input.component';
import { localToUtc, utcToLocal } from 'src/app/shared/lib/utilities';
import { EntityId } from 'src/app/shared/models/common';
import { ICreateLockDTO, ILockDTO, IUpdateLockDTO, LockOperatingMode } from 'src/app/shared/models/lock';

@Component({
  selector: 'admin-edit-lock-form',
  templateUrl: './edit-lock-form.component.html',
})
export class EditLockFormComponent implements OnInit {

  private readonly dialogService = inject(DialogService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly breakpointObserver = inject(BreakpointObserver);

  @Input() lock$: Observable<ILockDTO | undefined> = of();

  @Output() lockSubmitted = new EventEmitter<ICreateLockDTO | IUpdateLockDTO>();

  // TODO: Create custom date picker component, extract in similar way to text input
  // to reduce repeated code & make simplify using date controls
  readonly isSmallScreen$ = this.breakpointObserver.observe([Breakpoints.XSmall, Breakpoints.Small]).pipe(map(x => x.matches));

  currentLockLockerId?: EntityId;

  lockSerialNumberControl = new FormControl('', [Validators.required, Validators.maxLength(32), Validators.pattern(/^\d*?$/)]);
  lockInstallationDateLocalTimeControl = new FormControl(moment(moment.now()));
  lockOperatingModeControl = new FormControl(LockOperatingMode.Installation, Validators.required);

  lockForm = new UntypedFormGroup({
    'serialNumber': this.lockSerialNumberControl,
    'installationDateLocalTime': this.lockInstallationDateLocalTimeControl,
    'operatingMode': this.lockOperatingModeControl,
  });

  readonly lockFirmwareVersionSettings: ITextInputSettings = {
    name: 'firmwareVersion',
    maxLength: 256,
  };

  lockFirmwareVersionControl = textControlFromSettings(this.lockFirmwareVersionSettings, this.lockForm);

  readonly LockOperatingMode = LockOperatingMode;

  ngOnInit(): void {
    this.lock$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(lock => {
      if(lock) {
        this.currentLockLockerId = lock.lockerId;

        const { installationDateUtc, ...lockProperties } = lock;
        const installationDateLocalTime = utcToLocal(installationDateUtc);

        this.lockForm.patchValue({ installationDateLocalTime, ...lockProperties })
      }
    });
  }

  submitForm() {
    if(this.lockForm.invalid) {
      this.dialogService.alert("Ensure all required properties are filled in and valid", "Validation Error");
      return;
    }

    const { installationDateLocalTime, ...lock } = this.lockForm.value;
    const lockerId = this.currentLockLockerId;

    const installationDateUtc = localToUtc(installationDateLocalTime).toISOString();

    this.lockSubmitted.emit({ ...lock, installationDateUtc, lockerId } as ICreateLockDTO | IUpdateLockDTO);
  }
}
