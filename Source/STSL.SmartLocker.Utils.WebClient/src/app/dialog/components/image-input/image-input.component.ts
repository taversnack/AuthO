import { CommonModule } from '@angular/common';
import { Component, Input, Output, EventEmitter, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
import { MaterialModule } from 'src/app/material/material.module';

export interface IImageInputSettings {
  name: string;
  display?: string;
  accept?: string;
  maxSize?: number; // Maximum file size in megabytes
}

@Component({
  selector: 'app-image-input',
  standalone: true,
  imports: [CommonModule ,MaterialModule],
  templateUrl: './image-input.component.html',
})

export class ImageInputComponent {
    
    @Input() settings?: IImageInputSettings;

    @Output() imageSelected = new EventEmitter<File>();
    @Output() editCancelled = new EventEmitter();

    @ViewChild(ElementRef) fileInput? :ElementRef;
    selectedFile: File | undefined = undefined;

    imagePreviewUrl: string | null = null;
    
    fileError = false;
    fileErrorMessage?: string;
    isDragOver = false;

    onDragOver(event: DragEvent): void {
        event.preventDefault();
        event.stopPropagation();
    }

    onDragEnter(event: DragEvent): void {
        this.isDragOver = true;
    }

    onDrop(event: DragEvent): void {
        event.preventDefault();
        event.stopPropagation();

        this.isDragOver = false;
        const files = event.dataTransfer?.files;

        if (files) {
            this.selectedFile = files[0];
            this.validateImage();
        }
    }

    onDragLeave(event: DragEvent): void {
        this.isDragOver = false;
    }

    onFileSelected(event: Event) {
        const input = event.target as HTMLInputElement;
        this.selectedFile = input.files?.[0] as File;

        this.validateImage();
    }

    validateImage() {
        if (this.selectedFile) {
            if(this.isValidFileType(this.selectedFile.type)) {
                if(this.isMaxSizeValid(this.selectedFile.size)) {
                    this.fileError = false;
                    this.imagePreviewUrl = URL.createObjectURL(this.selectedFile);
                } else {
                    this.fileError = true;
                    this.imagePreviewUrl = null;
                    this.fileErrorMessage = 'Image size must be less than ' + this.settings?.maxSize + "MB";
                }
            } else {
                this.fileError = true;
                this.imagePreviewUrl = null;
                this.fileErrorMessage = 'Image type must be ' + this.settings?.accept;
            }
        }
    }

    submit() {
        if(this.selectedFile)
        {
            this.imageSelected.emit(this.selectedFile);
        }
    }

    cancel() {
        this.editCancelled.emit();
    }

    private isValidFileType(fileType: string): boolean {
        if (this.settings && this.settings.accept) 
        {
            const acceptedTypes = this.settings.accept.split(',').map(type => type.trim());
            return acceptedTypes.includes(fileType);
        }
        return true;
    }

    private isMaxSizeValid(fileSize: number): boolean {
        if (this.settings && this.settings.maxSize)
        {
            const maxSizeInBytes = this.settings.maxSize * 1024 * 1024; // convert MB to Bytes
            return fileSize <= maxSizeInBytes
        }
        return true;
    }
}