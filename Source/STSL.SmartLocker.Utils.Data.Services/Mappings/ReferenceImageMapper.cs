using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.Domain;
using STSL.SmartLocker.Utils.DTO;

namespace STSL.SmartLocker.Utils.Data.Services.Mappings;

internal sealed class MetaDataMapper : IMapsToDTO<MetaDataDTO, ImageMetaData>
{
    public static MetaDataDTO ToDTO(ImageMetaData entity) => new()
    {
        FileName = entity.FileName,
        PixelHeight = entity.PixelHeight,
        PixelWidth = entity.PixelWidth,
        ByteSize = entity.ByteSize,
        MimeType = entity.MimeType,
        UploadedByCardHolderEmail = entity.UploadedByCardHolderEmail,
        UploadedDate = entity.UploadedDate,
        AzureBlobName = entity.AzureBlobName,
    };
}

internal sealed class CreateMetaDataMapper : IMapsToEntity<CreateMetaDataDTO, ImageMetaData>
{
    public static ImageMetaData ToEntity(CreateMetaDataDTO dto) => new()
    {
        FileName = dto.FileName,
        PixelHeight = dto.PixelHeight,
        PixelWidth = dto.PixelWidth,
        ByteSize = dto.ByteSize,
        MimeType = dto.MimeType,
        UploadedByCardHolderEmail = dto.UploadedByCardHolderEmail,
        AzureBlobName = dto.AzureBlobName,
    };
}

internal sealed class ReferenceImageMapper<T> : IMapsToDTO<ReferenceImageDTO, T> where T : IEntityReferenceImage, IUsesGuidId, new()
{
    public static ReferenceImageDTO ToDTO(T entity) => new()
    {
        Id = entity.Id,
        EntityId = entity.EntityId,
        MetaData = MetaDataMapper.ToDTO(entity.MetaData)
    };

    public static ReferenceImageDTO ToDTOWithSASToken(T entity, string? token) => new()
    {
        Id = entity.Id,
        EntityId = entity.EntityId,
        MetaData = MetaDataMapper.ToDTO(entity.MetaData),
        BlobSASToken = token
    };
}

internal sealed class CreateReferenceImageMapper<T> : IMapsToEntity<CreateReferenceImageDTO, T> where T : IEntityReferenceImage, new()
{
    public static T ToEntity(CreateReferenceImageDTO dto) => new()
    {
        EntityId = dto.EntityId,
        MetaData = CreateMetaDataMapper.ToEntity(dto.MetaData)
    };
}
