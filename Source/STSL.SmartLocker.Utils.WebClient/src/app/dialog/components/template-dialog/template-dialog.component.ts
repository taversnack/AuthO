import { Component, TemplateRef, ViewContainerRef, inject } from '@angular/core';
import { DialogBase } from '../../directives/dialog-component-base.directive';

export interface ITemplateDialogContext<Data = any, Return = any> {
  $implicit: Data | null;
  close: (r?: Return) => void;
}

export type TemplateDialogRef<Data, Return> = TemplateRef<ITemplateDialogContext<Data, Return>>;

@Component({
  selector: 'app-template-dialog',
  templateUrl: './template-dialog.component.html',
})
export class TemplateDialogComponent<Data, Return> extends DialogBase<TemplateDialogComponent<Data, Return>, Data, Return> {

  private readonly viewContainerRef = inject(ViewContainerRef);

  readonly templateData: ITemplateDialogContext<Data, Return> = { $implicit: this.data, close: this.close };
}
