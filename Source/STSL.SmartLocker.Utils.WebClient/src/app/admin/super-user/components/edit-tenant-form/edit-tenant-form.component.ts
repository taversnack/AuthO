import { Component, DestroyRef, ElementRef, EventEmitter, Input, OnInit, Output, ViewChild, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, UntypedFormGroup, Validators } from '@angular/forms';
import { Observable, of } from 'rxjs';
import { DialogService } from 'src/app/dialog/services/dialog.service';
import { ITextInputSettings, textControlFromSettings } from 'src/app/shared/components/text-input/text-input.component';
import { ICreateTenantDTO, ITenantDTO, IUpdateTenantDTO } from 'src/app/shared/models/tenant';

@Component({
  selector: 'super-user-edit-tenant-form',
  templateUrl: './edit-tenant-form.component.html',
})
export class EditTenantFormComponent implements OnInit {

  private readonly dialogService = inject(DialogService);
  private readonly destroyRef = inject(DestroyRef);

  @ViewChild('logoInput') logoInput?: ElementRef<HTMLInputElement>;

  @Input() tenant$: Observable<ITenantDTO | undefined> = of();

  @Output() tenantSubmitted = new EventEmitter<ICreateTenantDTO | IUpdateTenantDTO>();
  @Output() cancelEdits = new EventEmitter<void>();

  hasTenant = false;

  readonly tenantNameSettings: ITextInputSettings = {
    name: 'name',
    required: true,
    maxLength: 256,
  };

  readonly tenantCardHolderAliasSingularSettings: ITextInputSettings = {
    name: 'cardHolderAliasSingular',
    maxLength: 256,
    placeholder: 'Card Holder',
  };

  readonly tenantCardHolderAliasPluralSettings: ITextInputSettings = {
    name: 'cardHolderAliasPlural',
    maxLength: 256,
    placeholder: 'Card Holders',
  };

  readonly tenantUniqueIdentifierAliasSettings: ITextInputSettings = {
    name: 'cardHolderUniqueIdentifierAlias',
    maxLength: 256,
    placeholder: 'Unique Identifier',
  }

  readonly tenantHelpPortalUrlSettings: ITextInputSettings = {
    name: 'helpPortalUrl',
    maxLength: 1024,
    typeOverride: 'url',
  };

  tenantAllowLockUpdates = new FormControl(false, Validators.required);

  tenantForm = new UntypedFormGroup({
    'allowLockUpdates': this.tenantAllowLockUpdates,
  });

  tenantNameControl = textControlFromSettings(this.tenantNameSettings, this.tenantForm);
  tenantCardHolderAliasSingularControl = textControlFromSettings(this.tenantCardHolderAliasSingularSettings, this.tenantForm);
  tenantCardHolderAliasPluralControl = textControlFromSettings(this.tenantCardHolderAliasPluralSettings, this.tenantForm);
  tenantUniqueIdentifierAliasControl = textControlFromSettings(this.tenantUniqueIdentifierAliasSettings, this.tenantForm);
  tenantHelpPortalUrlControl = textControlFromSettings(this.tenantHelpPortalUrlSettings, this.tenantForm);

  tenantLogoSrc?: string;
  tenantLogoMimeType?: string;

  tenantLogoPreviewSrc?: string;

  readonly acceptedLogoMimeTypes = 'image/jpeg';

  ngOnInit(): void {
    this.tenant$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(tenant => {
      if(tenant) {
        this.hasTenant = true;
        this.tenantForm.patchValue(tenant);
        this.tenantLogoSrc = tenant.logo;
        this.tenantLogoMimeType = tenant.logoMimeType;
        this.tenantLogoPreviewSrc = this.tenantLogoSrc && this.tenantLogoMimeType ?
        `data:${this.tenantLogoMimeType};base64,${this.tenantLogoSrc}`
        :
        '';
      } else {
        this.hasTenant = false;
        this.tenantForm.patchValue({ name: '', cardHolderAliasSingular: '', cardHolderAliasPlural: '', cardHolderUniqueIdentifierAlias: '', helpPortalUrl: '', allowLockUpdates: false });
        this.tenantLogoSrc = this.tenantLogoMimeType = undefined;
        this.tenantLogoPreviewSrc = '';
      }
    });
  }

  setLogo() {
    const file = this.logoInput?.nativeElement.files?.item(0);
    // 2 MB
    const maxFileSizeMegaBytes = 2;
    const maxFileSizeBytes = 1024 * 1024 * maxFileSizeMegaBytes;

    if (!file) {
      this.dialogService.error('Problem processing logo file');
      return;
    }

    if (file.size > maxFileSizeBytes) {
      this.dialogService.error(`File too large, please ensure the file size is less than ${maxFileSizeMegaBytes}MB`);
      return;
    }

    this.tenantLogoMimeType = file.type;

    const reader = new FileReader();
    reader.onload = e => {
      if (e.target?.result) {
        this.tenantLogoSrc = e.target.result.toString().replace(/\S*(base64,)/, '');
        this.tenantLogoPreviewSrc = `data:${this.tenantLogoMimeType};base64,${this.tenantLogoSrc}`;
      }
    };

    // reader.onprogress = e => {
    //   this.imageLoadedProgress = e.loaded / e.total;
    // }

    reader.readAsDataURL(file);
  }

  submitForm() {
    if(this.tenantForm.invalid) {
      this.dialogService.error("Ensure all required properties are filled in and valid", "Validation Error");
      return;
    }

    const tenant = this.tenantForm.value as ICreateTenantDTO | IUpdateTenantDTO;
    if(this.tenantLogoSrc && this.tenantLogoMimeType) {
      tenant.logo = this.tenantLogoSrc;
      tenant.logoMimeType = this.tenantLogoMimeType;
    }
    if(!tenant.cardHolderAliasSingular) {
      delete tenant.cardHolderAliasSingular;
    }
    if(!tenant.cardHolderAliasPlural) {
      delete tenant.cardHolderAliasPlural;
    }
    if(!tenant.cardHolderUniqueIdentifierAlias) {
      delete tenant.cardHolderUniqueIdentifierAlias;
    }
    this.tenantSubmitted.emit(tenant);
  }

  cancelEditing() {
    this.cancelEdits.emit();
  }

  warnUserAboutTogglingLockUpdates($event: void) {
    if(!this.hasTenant) {
      return;
    }

    const warningMessage = 'Are you sure you wish to change allowing lock updates? This can affect actual locks even in a development environment';
    this.dialogService.confirm(warningMessage, 'Warning').pipe(takeUntilDestroyed(this.destroyRef)).subscribe(confirmed => {
      if(!confirmed) {
        this.tenantAllowLockUpdates.patchValue(!this.tenantAllowLockUpdates.value);
      }
    })
  }
}
