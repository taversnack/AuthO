import { Component } from '@angular/core';
import { ThemePalette } from '@angular/material/core';
import { DialogBase } from '../../directives/dialog-component-base.directive';

export interface IConfirmationDialogData {
  message?: string | string[];
  confirmActionLabel?: string;
  confirmActionThemeColour?: ThemePalette;
  cancelActionLabel?: string;
  cancelActionThemeColour?: ThemePalette;
}

const defaultConfirmationDialogData: Required<IConfirmationDialogData> = {
  message: 'Are you sure?',
  confirmActionLabel: 'Confirm',
  confirmActionThemeColour: 'primary',
  cancelActionLabel: 'Cancel',
  cancelActionThemeColour: 'warn',
}

@Component({
  selector: 'app-confirmation-dialog',
  templateUrl: './confirmation-dialog.component.html',
})
export class ConfirmationDialogComponent extends DialogBase<ConfirmationDialogComponent, IConfirmationDialogData, boolean> {
  readonly settings = { ...defaultConfirmationDialogData, ...this.data };

  readonly paragraphs = typeof this.settings.message === 'string' ? [this.settings.message] : this.settings.message;
}
