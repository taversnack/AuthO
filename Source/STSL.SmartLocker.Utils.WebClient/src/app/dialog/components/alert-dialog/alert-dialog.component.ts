import { Component } from '@angular/core';
import { ThemePalette } from '@angular/material/core';
import { DialogBase } from '../../directives/dialog-component-base.directive';

export interface IAlertDialogData {
  message?: string | string[];
  actionLabel: string;
  actionThemeColour: ThemePalette;
}

@Component({
  selector: 'app-alert-dialog',
  templateUrl: './alert-dialog.component.html',
})
export class AlertDialogComponent extends DialogBase<AlertDialogComponent, IAlertDialogData> {
  readonly paragraphs = this.data?.message === undefined ? [] : Array.isArray(this.data?.message) ? this.data?.message : [this.data?.message];
}
