import { ComponentType } from '@angular/cdk/portal';
import { inject, Injectable, TemplateRef } from '@angular/core';
import { ThemePalette } from '@angular/material/core';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import { map, Observable } from 'rxjs';
import { AlertDialogComponent, IAlertDialogData } from '../components/alert-dialog/alert-dialog.component';
import { ConfirmationDialogComponent, IConfirmationDialogData } from '../components/confirmation-dialog/confirmation-dialog.component';
import { DialogContainerComponent } from '../components/dialog-container/dialog-container.component';
import { IIsLoadingDialogData, IsLoadingDialogComponent } from '../components/is-loading-dialog/is-loading-dialog.component';
import { IOpenFileDialogData, OpenFileDialogComponent } from '../components/open-file-dialog/open-file-dialog.component';
import { DialogBase } from '../directives/dialog-component-base.directive';
import { IDialog } from '../models/dialog';

export interface ICustomDialogSettings {
  width?: string;
  height?: string;
  minWidth?: number | string;
  minHeight?: number | string;
  maxWidth?: number | string;
  maxHeight?: number | string;
  autoFocus?: string;
}

export interface IAlertDialogSettings {
  actionLabel?: string;
  actionThemeColour?: ThemePalette;
}

@Injectable({
  providedIn: 'root'
})
export class DialogService {

  private readonly dialog = inject(MatDialog);

  private readonly defaultDialogSettings: MatDialogConfig = {
    width: 'calc(100% - 1rem)',
    maxWidth: 640,
    maxHeight: 480,
    ariaLabelledBy: null,
    // panelClass: ['animate-pulse', 'focus-within:animate-none'],
    backdropClass: ['bg-gradient-to-tr', 'from-primary-900/75', 'to-primary-400/25', 'backdrop-blur-sm'],
    restoreFocus: false,
    // enterAnimationDuration: 200,
    // exitAnimationDuration: 200,
  }

  // private readonly defaultSnackBarSettings: MatSnackBarConfig = {
  //   duration: 4000,
  // };

  // NOTE: Requires i18n
  private readonly defaultActionLabel = 'Ok';

  alert(message?: string | string[], title: string = 'Alert', settings?: IAlertDialogSettings): Observable<undefined> {
    return this.open<undefined, AlertDialogComponent, IAlertDialogData>(AlertDialogComponent, title, { message, ...{ actionLabel: this.defaultActionLabel, actionThemeColour: 'primary' }, ...settings });
  }

  error(message?: string | string[], title: string = 'Error', settings?: IAlertDialogSettings): Observable<undefined> {
    return this.open<undefined, AlertDialogComponent, IAlertDialogData>(AlertDialogComponent, title, { message, ...{ actionLabel: this.defaultActionLabel, actionThemeColour: 'warn' }, ...settings });
  }

  confirm(message?: string | string[], title: string = 'Are you sure?', settings?: IConfirmationDialogData): Observable<boolean> {
    return this.open<boolean, ConfirmationDialogComponent, IConfirmationDialogData>(ConfirmationDialogComponent, title, { ...settings, message })
      .pipe(map(x => x ?? false));
  }

  // notify(message: string, duration: number = 4000) {
  //   this.snackBar.open(message, this.defaultActionLabel, { ...this.defaultSnackBarSettings, duration });
  // }

  openFileUpload(settings: IOpenFileDialogData = {}, title: string = 'Select File(s)'): Observable<File[] | undefined> {
    return this.open<File[], OpenFileDialogComponent, IOpenFileDialogData>(OpenFileDialogComponent, title, settings);
  }

  openTemplate<T = any>(template: TemplateRef<T>, title: string) {
    // TODO: Create template dialog component (useful for quick simple dialogs which don't quite fit alert / confirm etc)
  }

  // showIsLoadingCancellable(isLoading$: Observable<boolean>): Observable<boolean> {
  // }

  showIsLoading(isLoading$: Observable<boolean>, useAlertMessage?: string) {
    // TODO: Create full screen loading spinner component (must ensure API service has timeout measures working correctly)
    this.dialog.open<DialogBase<IsLoadingDialogComponent, IIsLoadingDialogData>, IIsLoadingDialogData>(
      IsLoadingDialogComponent,
      {
        data: { isLoading$, useAlertMessage },
        backdropClass: ['bg-gradient-to-tr', 'from-primary-900/75', 'to-primary-400/25', 'backdrop-blur-sm'],
        disableClose: true,
        ariaLabel: 'Loading, please wait',
      });
  }

  open<R, T extends DialogBase<T, D, R>, D = null>(component: ComponentType<T>, title?: string, data: D | null = null): Observable<R | undefined> {
    return this.dialog.open<DialogContainerComponent<T, D, R>, IDialog<T, D | null>, R>(
      DialogContainerComponent<T, D, R>,
      {
        data: { title, data, component },
        ariaLabel: title,
        ...this.defaultDialogSettings
      }
    ).afterClosed();
  }

  openLarge<R, T extends DialogBase<T, D, R>, D = null>(component: ComponentType<T>, title?: string, data: D | null = null): Observable<R | undefined> {
    return this.dialog.open<DialogContainerComponent<T, D, R>, IDialog<T, D | null>, R>(
      DialogContainerComponent<T, D, R>,
      {
        data: { title, data, component },
        ariaLabel: title,
        ...this.defaultDialogSettings,
        height: 'calc(100% - 1rem)',
        maxWidth: undefined,
        maxHeight: undefined,
      }
    ).afterClosed();
  }

  openMedium<R, T extends DialogBase<T, D, R>, D = null>(component: ComponentType<T>, title?: string, data: D | null = null): Observable<R | undefined> {
    return this.dialog.open<DialogContainerComponent<T, D, R>, IDialog<T, D | null>, R>(
      DialogContainerComponent<T, D, R>,
      {
        data: { title, data, component },
        ariaLabel: title,
        ...this.defaultDialogSettings,
        height: 'clamp(20rem, calc(75% - 1rem), 35rem)',
        width: 'clamp(40rem, calc(75% - 1rem), 70rem)',
        maxWidth: undefined,
        maxHeight: undefined,
      }
    ).afterClosed();
  }

  openCustom<R, T extends DialogBase<T, D, R>, D = null>(dialogOptions: ICustomDialogSettings, component: ComponentType<T>, title?: string, data: D | null = null): Observable<R | undefined> {
    return this.dialog.open<DialogContainerComponent<T, D, R>, IDialog<T, D | null>, R>(
      DialogContainerComponent<T, D, R>,
      {
        data: { title, data, component },
        ariaLabel: title,
        ...this.defaultDialogSettings,
        ...dialogOptions,
      }
    ).afterClosed();
  }
}
