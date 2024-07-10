import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, ValidatorFn, Validators } from '@angular/forms';
import { MaterialModule } from 'src/app/material/material.module';
import { camelCaseToSpacedPascalCase } from '../../lib/utilities';

export interface ITextInputSettings {
  name: string;
  display?: string;
  placeholder?: string;
  required?: boolean;
  maxLength?: number;
  minLength?: number;
  typeOverride?: 'email' | 'url' | 'password' | 'numeric';
}

@Component({
  selector: 'app-text-input',
  standalone: true,
  imports: [CommonModule, MaterialModule, ReactiveFormsModule],
  templateUrl: './text-input.component.html',
})
export class TextInputComponent {
  @Input() settings?: ITextInputSettings;
  @Input() control?: FormControl;
}

export const textControlFromSettings = (settings: ITextInputSettings, formGroup?: FormGroup): FormControl => {
  settings.display ??= camelCaseToSpacedPascalCase(settings.name);
  if(settings.minLength && settings.maxLength && settings.minLength > settings.maxLength) {
    throw new Error(`minLength greater than maxLength for ${settings.name}`);
  }
  // setup validators as per settings
  const validators: ValidatorFn[] = [];
  if(settings?.required) {
    validators.push(Validators.required);
  }
  if(settings?.maxLength) {
    validators.push(Validators.maxLength(settings?.maxLength));
  }
  if(settings?.minLength) {
    validators.push(Validators.minLength(settings?.minLength));
  }

  if(settings?.typeOverride === 'email') {
    validators.push(Validators.email);
  }

  if(settings?.typeOverride === 'numeric') {
    validators.push(Validators.pattern(/^\d+$/));
  }

  const formControl = new FormControl('', validators);

  if(formGroup && settings?.name) {
    formGroup.addControl(settings.name, formControl);
  }

  return formControl;
}
