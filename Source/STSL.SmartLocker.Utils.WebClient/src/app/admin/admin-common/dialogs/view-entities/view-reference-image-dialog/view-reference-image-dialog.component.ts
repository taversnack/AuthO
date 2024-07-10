import { Component, OnInit} from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { DialogBase } from 'src/app/dialog/directives/dialog-component-base.directive';

export interface IViewReferenceImageDialogData {
  blobSASToken?: string;
}

@Component({
  selector: 'reference-image-dialog',
  templateUrl: './view-reference-image-dialog.component.html',
})
export class ViewReferenceImageDialogComponent extends DialogBase<ViewReferenceImageDialogComponent, IViewReferenceImageDialogData>{

  private readonly imageSubject = new BehaviorSubject<string | undefined>(this.data?.blobSASToken);
  readonly image$ = this.imageSubject.asObservable();

}