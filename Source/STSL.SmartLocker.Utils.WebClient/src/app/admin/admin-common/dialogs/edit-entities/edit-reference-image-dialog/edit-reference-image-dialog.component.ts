import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { DialogBase } from 'src/app/dialog/directives/dialog-component-base.directive';
import { EditEntityDialogState } from '../../../models/entity-forms';
import { IReferenceMapImageDTO, ICreateReferenceImageDTO, IViewReferenceImageDTO, IUpdateReferenceImageDTO } from 'src/app/shared/models/reference-image';
import { ICrudEndpoints } from 'src/app/shared/models/api';
import { IHasReferenceImage, IUsesGuidId } from 'src/app/shared/models/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

export interface IEditReferenceImageDialogData<T extends IUsesGuidId & IHasReferenceImage> {
    entity?: T; // Location / LockerBank etc
    endPoints?: ICrudEndpoints<IViewReferenceImageDTO, ICreateReferenceImageDTO, IUpdateReferenceImageDTO>
}

@Component({
  selector: 'admin-edit-reference-image-dialog',
  templateUrl: './edit-reference-image-dialog.component.html',
})
export class EditReferenceImageDialogComponent<T extends IUsesGuidId & IHasReferenceImage> extends DialogBase<EditReferenceImageDialogComponent<T>, IEditReferenceImageDialogData<T>, IViewReferenceImageDTO> implements OnInit {

  private readonly destroyRef = inject(DestroyRef);

  private readonly entitySubject = new BehaviorSubject<T | undefined>(this.data?.entity);
  readonly entity$ = this.entitySubject.asObservable();

  private readonly imageSubject = new BehaviorSubject<IViewReferenceImageDTO | null>(null);
  readonly image$ = this.imageSubject.asObservable();

  dialogState = EditEntityDialogState.SavingData;
  readonly EditEntityDialogState = EditEntityDialogState;

  readonly settings: IEditReferenceImageDialogData<T> = {
   entity: undefined,
   endPoints: undefined,
   ...this.data
  };

  ngOnInit(): void { 
    this.entitySubject.subscribe(entity => {
      const currentEntity = entity;
      if(currentEntity && this.settings.endPoints)
      {
        this.settings.endPoints.getSingle(currentEntity.id).subscribe(image =>{
          if(image) {
            this.imageSubject.next(image);
          }
          this.dialogState = EditEntityDialogState.EditingForm
        });
      }
    });
  }

  createReferenceImage(referenceMapImage: IReferenceMapImageDTO) {
    const error = (err: Error) => { this.notifyFailed(); throw err };

    this.dialogState = EditEntityDialogState.SavingData;
    const currentEntity = this.entitySubject.getValue();

    if(currentEntity && this.settings.endPoints)
    {
      const createReferenceImageDTO: ICreateReferenceImageDTO = {
        entityId: currentEntity.id,
        azureBlobDTO: referenceMapImage.azureBlobDTO,
        metaData: referenceMapImage.metaData,  
      };

      this.settings.endPoints.createSingle(createReferenceImageDTO)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe(
          { 
            next: image => {
              this.imageSubject.next(image);
              const updatedEntity = {...currentEntity, referenceImageId: image?.id}
              this.entitySubject.next(updatedEntity);
              this.dialogState = EditEntityDialogState.EditingForm;
            },
            error
          });
    }
  }

  deleteReferenceImage() {
    const currentEntity = this.entitySubject.getValue();

    if(currentEntity?.referenceImageId && this.settings.endPoints)
    {
      this.settings.endPoints.deleteSingle(currentEntity.referenceImageId)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe(deleted => {
          if(deleted)
          {
            // update values here to reset "edit mode"
            this.imageSubject.next(null);
            const updatedEntity = {...currentEntity, referenceImageId: undefined}
            this.entitySubject.next(updatedEntity);
          }
        });
    }
  }

  notifyFailed() {
    this.dialogState = EditEntityDialogState.SaveFailed;
  }
}
