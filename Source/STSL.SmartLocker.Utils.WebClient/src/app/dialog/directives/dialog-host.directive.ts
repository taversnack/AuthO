import { Directive, ViewContainerRef } from '@angular/core';

@Directive({
  selector: '[dialogHost]',
})
export class DialogHostDirective {
  constructor(public viewContainerRef: ViewContainerRef) {}
}
