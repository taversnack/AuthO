import { Component, OnInit, Output, EventEmitter, Input, inject} from '@angular/core';
import { Observable, combineLatest, of } from 'rxjs';
import { AuthService } from 'src/app/auth/services/auth.service';
import { IAzureBlobDTO, IImageMetaData, IReferenceMapImageDTO, IViewReferenceImageDTO } from 'src/app/shared/models/reference-image';
import { BehaviorSubject } from 'rxjs';
import { IImageInputSettings } from 'src/app/dialog/components/image-input/image-input.component';

@Component({
  selector: 'admin-edit-reference-image',
  templateUrl: './edit-reference-image.component.html',
})
export class EditReferenceImageComponent implements OnInit {
  private readonly authService = inject(AuthService);

  private metaDataSubject = new BehaviorSubject<IImageMetaData | undefined>(undefined);
  readonly metaData$ = this.metaDataSubject.asObservable();

  private azureBlobSubject = new BehaviorSubject<IAzureBlobDTO | undefined>(undefined);
  readonly azureBlob$ = this.azureBlobSubject.asObservable();

  private imageUri = new BehaviorSubject<string | undefined>(undefined);
  readonly imageUri$ = this.imageUri.asObservable();

  @Input() set image(value: Observable<IViewReferenceImageDTO | null>) {
    this._image = value;
    this.updateShowEditMode();
  }

  get image(): Observable<IViewReferenceImageDTO | null> {
    return this._image;
  }

  // observable for setting edit mode
  private _image: Observable<IViewReferenceImageDTO | null> = of(null);

  @Output() referenceImageSubmitted = new EventEmitter<IReferenceMapImageDTO>();
  @Output() referenceImageDeleted = new EventEmitter();
  @Output() referenceImageCancelled = new EventEmitter();

  showEditMode = true;

  imageInputSettings: IImageInputSettings = {
    name: 'referenceMapImage',
    display: 'Browse',
    accept: 'image/jpeg, image/png',
    maxSize: 2 // Max sizes in megabytes
  }

  ngOnInit(): void {
    this.updateShowEditMode();

    this.image.subscribe(image => {
      if (image?.blobSASToken) {
        this.imageUri.next(image?.blobSASToken);
      }
    });
  }

  toggleEditMode() {
    this.showEditMode = !this.showEditMode;
  }

  deleteImage() {
    this.referenceImageDeleted.emit();
  }
  
  cancelEdit()
  {  
    this.referenceImageCancelled.emit();
  }

  handleSelectedImage(file: File) {

    this.createMetaDataDTO(file);
    this.createAzureBlobDTO(file);

    this.emitReferenceMapImageDTO();
  }


  private emitReferenceMapImageDTO()
  {
    combineLatest([this.metaData$, this.azureBlob$]).subscribe(([metaData, azureBlobDTO]) => {
      if (metaData && azureBlobDTO)
      {
        const referenceMapImageDTO: IReferenceMapImageDTO = {
          azureBlobDTO: azureBlobDTO,
          metaData: metaData
        };
        
        this.referenceImageSubmitted.emit(referenceMapImageDTO);
      }
    });
  }

  private updateShowEditMode() {
    this._image.subscribe(image => {
      this.showEditMode = image === null;
    });
  }

  private createMetaDataDTO(file: File)
  {
    const image = new Image();
    image.onload = () => {
      const width = image.width;
      const height = image.height;
      const metaData: IImageMetaData = {
        fileName: file.name,
        pixelHeight: height,
        pixelWidth: width,
        byteSize: file.size,
        mimeType: file.type,
      };
      
      this.metaDataSubject.next(metaData);
    };

    image.src = URL.createObjectURL(file);
  }

  private createAzureBlobDTO(file: File)
  {
    const reader = new FileReader();
    reader.onload = (e) => {
      if (e.target?.result) {
        const azureBlobDTO: IAzureBlobDTO = {
          fileName: file.name,
          blobContentType: file.type,
          blobData: e.target.result.toString().replace(/\S*(base64,)/, '')
        };

      this.azureBlobSubject.next(azureBlobDTO);
      }
    };

    reader.readAsDataURL(file);
  }
}