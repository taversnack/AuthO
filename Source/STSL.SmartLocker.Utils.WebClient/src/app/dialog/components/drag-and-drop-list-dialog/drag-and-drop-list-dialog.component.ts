import { CdkDragDrop, DropListOrientation, moveItemInArray } from '@angular/cdk/drag-drop';
import { Component } from '@angular/core';
import { DialogBase } from '../../directives/dialog-component-base.directive';

export enum Axis2D {
  Vertical = 1 << 1,
  Horizontal = 1 << 2,
}

export interface IDragAndDropListDialogData {
  listItems: string[];
  displayMode?: Axis2D;
}

const defaultDragAndDropListDialogData: Required<IDragAndDropListDialogData> = {
  listItems: [],
  displayMode: Axis2D.Horizontal,
}

@Component({
  selector: 'app-drag-and-drop-list-dialog',
  templateUrl: './drag-and-drop-list-dialog.component.html',
})
export class DragAndDropListDialogComponent extends DialogBase<DragAndDropListDialogComponent, IDragAndDropListDialogData, string[]> {

  readonly settings: Required<IDragAndDropListDialogData> = { ...defaultDragAndDropListDialogData, ...this.data };

  dropListItems = [...this.data?.listItems ?? []];

  readonly Axis2D = Axis2D;

  readonly displayModeString: DropListOrientation = (() => {
    switch(this.data?.displayMode) {
      case Axis2D.Vertical: return 'vertical';
      default: return 'horizontal';
    }
  })();

  moveListItem(event: CdkDragDrop<string[]>) {
    moveItemInArray(this.dropListItems, event.previousIndex, event.currentIndex);
  }
}
