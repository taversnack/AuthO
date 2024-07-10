import { ComponentType } from '@angular/cdk/portal';
import { Component, inject, Injector, OnInit, ViewChild } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { DialogBase } from '../../directives/dialog-component-base.directive';
import { DialogHostDirective } from '../../directives/dialog-host.directive';
import { IDialog } from '../../models/dialog';

@Component({
  selector: 'app-dialog-container',
  templateUrl: './dialog-container.component.html',
  host: { 'class': 'relative flex flex-col h-full' }
})
export class DialogContainerComponent<T extends DialogBase<T, D, R>, D, R> implements OnInit {

  data: IDialog<T, D> = inject(MAT_DIALOG_DATA);
  dialogRef: MatDialogRef<DialogContainerComponent<T, D, R>, R> | null = inject(MatDialogRef<DialogContainerComponent<T, D, R>, R>);

  title?: string = this.data.title;
  component?: ComponentType<T> = this.data.component;

  @ViewChild(DialogHostDirective, { static: true }) dialogHost!: DialogHostDirective;

  ngOnInit(): void {
    if(!this.component) {
      console.error('Component not supplied on init of DialogContainerComponent');
      return;
    }

    const injector = Injector.create({
      providers: [
        { provide: MatDialogRef<T, R>, useValue: this.dialogRef },
        { provide: MAT_DIALOG_DATA, useValue: this.data.data },
      ],
    });

    this.dialogHost.viewContainerRef.createComponent(this.component, { injector });
  }

  close() {
    this.dialogRef?.close();
  }
}
