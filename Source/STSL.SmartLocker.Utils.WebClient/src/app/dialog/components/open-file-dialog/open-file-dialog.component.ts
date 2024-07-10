import { Component } from '@angular/core';
import { DialogBase } from '../../directives/dialog-component-base.directive';

export interface IOpenFileDialogData {
  accept?: string[];
  maxFileCount?: number;
}

const defaultOpenFileDialogData: Required<IOpenFileDialogData> = {
  accept: [],
  maxFileCount: 1,
}

@Component({
  selector: 'app-open-file-dialog',
  templateUrl: './open-file-dialog.component.html',
})
export class OpenFileDialogComponent extends DialogBase<OpenFileDialogComponent, IOpenFileDialogData, File[]> {

  readonly settings = { ...defaultOpenFileDialogData, ...this.data };

  isDraggingOverDropArea: boolean = false;

  filesAreAcceptable: boolean = false;

  fileDescriptions: string[] = [];

  private files: File[] = [];

  filesUploaded(event: Event) {
    if(!event.target) {
      return;
    }

    const target = event.target as HTMLInputElement;
    const files = Array.from(target.files ?? []);

    this.useFiles(files);
  }

  filesDropped(event: DragEvent) {
    event.preventDefault();
    this.isDraggingOverDropArea = false;

    let files: File[] = [];
    if(event.dataTransfer?.items) {
      files = Array.from(event.dataTransfer.items).map(x => x.getAsFile()).filter((x): x is File => x !== null);
    } else if(event.dataTransfer?.files) {
      files = Array.from(event.dataTransfer.files);
    }

    this.useFiles(files);
  }

  private useFiles(files: File[]) {

    this.fileDescriptions = files.map(file => `${file.name} - (${file.type}) ${(file.size / 1024).toFixed(2)}KB`);

    if(files.length && files.length <= this.settings.maxFileCount) {
      this.files = files;
      this.filesAreAcceptable = true;
    }
  }

  filesDraggedOver(event: DragEvent) {
    event.stopPropagation();
    event.preventDefault();
  }

  fileDragEnter(event: DragEvent) {
    this.isDraggingOverDropArea = true;
  }

  fileDragLeave(event: DragEvent) {
    this.isDraggingOverDropArea = false;
  }

  confirmFiles() {
    if(this.filesAreAcceptable) {
      this.close(this.files);
    }
  }
}
