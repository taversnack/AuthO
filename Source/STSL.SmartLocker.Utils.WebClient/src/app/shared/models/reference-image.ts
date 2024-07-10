import { IUsesGuidId } from "./common";

export type Base64EncodedImage = string;

// Azure Blob & Metadata
export interface IAzureBlobDTO {
    fileName: string;
    blobContentType: string;
    blobData: Base64EncodedImage;
}

export interface IImageMetaData {
    fileName: string;
    pixelWidth: number;
    pixelHeight: number;
    byteSize: number;
    mimeType: string;
    uploadedByCardHolderEmail?: string;
}

// Reference Images
export interface IReferenceMapImageDTO {
    azureBlobDTO: IAzureBlobDTO;
    metaData: IImageMetaData;
}

export interface ICreateReferenceImageDTO {
    entityId: string;
    azureBlobDTO: IAzureBlobDTO;
    metaData: IImageMetaData;
}

export interface IUpdateReferenceImageDTO {
    entityId: string;
    azureBlobDTO: IAzureBlobDTO;
    metaData: IImageMetaData;
}

export interface IViewReferenceImageDTO extends IUsesGuidId {
    entityId: string,
    metaData: IImageMetaData
    blobSASToken?: string
}
