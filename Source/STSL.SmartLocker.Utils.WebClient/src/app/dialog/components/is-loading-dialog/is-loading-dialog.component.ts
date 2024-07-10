import { Component, DestroyRef, HostBinding, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Observable } from 'rxjs';
import { DialogBase } from '../../directives/dialog-component-base.directive';

export interface IIsLoadingDialogData {
  isLoading$: Observable<boolean>;
  useAlertMessage?: string;
}

@Component({
  selector: 'app-is-loading-dialog',
  templateUrl: './is-loading-dialog.component.html',
  host: { 'class': 'fixed -translate-x-1/2 -translate-y-1/2' }
})
export class IsLoadingDialogComponent extends DialogBase<IsLoadingDialogComponent, IIsLoadingDialogData> implements OnInit {

  private readonly destroyRef = inject(DestroyRef);

  @HostBinding('class') isFullScreen = this.data?.useAlertMessage ? 'flex flex-col items-center bg-white rounded p-4 gap-4' : 'fixed -translate-x-1/2 -translate-y-1/2';

  ngOnInit(): void {
    if(!this.data) {
      this.close();
    }

    this.data?.isLoading$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(isLoading => {
      if(!isLoading) {
        this.close();
      }
    })
  }
}
