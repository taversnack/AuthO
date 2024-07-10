import { Directive, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

@Directive()
export abstract class DialogBase<T, D = null, R = undefined> {

  private readonly dialogRef: MatDialogRef<T, R> | null = inject(MatDialogRef<T, R>);
  protected data: D | null = inject(MAT_DIALOG_DATA);

  protected close(returnedData?: R) {
    this.dialogRef?.close(returnedData);
  }
}
