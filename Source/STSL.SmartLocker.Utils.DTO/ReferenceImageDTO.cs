using STSL.SmartLocker.Utils.Common.Data;

namespace STSL.SmartLocker.Utils.DTO;

public readonly record struct CreateAzureBlobDTO(
     string FileName,
     string BlobContentType,
     byte[] BlobData
);

public readonly record struct CreateMetaDataDTO(
    string FileName,
    int PixelHeight,
    int PixelWidth,
    int ByteSize,
    string MimeType,
    string? UploadedByCardHolderEmail,
    DateTimeOffset UploadedDate,
    string? AzureBlobName
);

public readonly record struct MetaDataDTO(
    string FileName,
    int PixelHeight,
    int PixelWidth,
    int ByteSize,
    string MimeType,
    string? UploadedByCardHolderEmail,
    DateTimeOffset UploadedDate,
    string? AzureBlobName
);

public readonly record struct CreateReferenceImageDTO(
    Guid EntityId,
    CreateMetaDataDTO MetaData,
    CreateAzureBlobDTO AzureBlobDTO
);

public readonly record struct UpdateReferenceImageDTO(
    Guid EntityId,
    CreateMetaDataDTO MetaData,
    CreateAzureBlobDTO AzureBlobDTO
);

public readonly record struct ReferenceImageDTO(
    Guid Id,
    Guid? EntityId,
    MetaDataDTO MetaData,
    string? BlobSASToken
);
