import { Overlay } from '@angular/cdk/overlay';
import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { MAT_DIALOG_SCROLL_STRATEGY } from '@angular/material/dialog';
import { MaterialModule } from '../material/material.module';
import { ConfirmationDialogComponent } from './components/confirmation-dialog/confirmation-dialog.component';
import { DialogContainerComponent } from './components/dialog-container/dialog-container.component';
import { DialogHostDirective } from './directives/dialog-host.directive';
import { AlertDialogComponent } from './components/alert-dialog/alert-dialog.component';
import { OpenFileDialogComponent } from './components/open-file-dialog/open-file-dialog.component';
import { DragAndDropListDialogComponent } from './components/drag-and-drop-list-dialog/drag-and-drop-list-dialog.component';
import { IsLoadingDialogComponent } from './components/is-loading-dialog/is-loading-dialog.component';
import { TemplateDialogComponent } from './components/template-dialog/template-dialog.component';

const publicComponents = [
  ConfirmationDialogComponent
];

const scrollStrategyFactory = (overlay: Overlay) => () => overlay.scrollStrategies.block();

@NgModule({
  declarations: [
    ...publicComponents,
    DialogContainerComponent,
    DialogHostDirective,
    AlertDialogComponent,
    OpenFileDialogComponent,
    DragAndDropListDialogComponent,
    IsLoadingDialogComponent,
    TemplateDialogComponent,
  ],
  exports: publicComponents,
  imports: [
    CommonModule,
    MaterialModule,
  ],
  providers: [
    { provide: MAT_DIALOG_SCROLL_STRATEGY, useFactory: scrollStrategyFactory, deps: [Overlay] },
  ]
})
export class DialogModule {}
