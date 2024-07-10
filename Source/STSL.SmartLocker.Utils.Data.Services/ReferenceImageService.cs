using STSL.SmartLocker.Utils.Data.Services.Contracts;
using STSL.SmartLocker.Utils.Data.Services.Mappings;
using STSL.SmartLocker.Utils.Domain;
using STSL.SmartLocker.Utils.DTO;
using STSL.SmartLocker.Utils.Common.Exceptions;
using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;

namespace STSL.SmartLocker.Utils.Data.Services;

internal class ReferenceImageService<T, E> : IReferenceImageService<T, E> 
    where T : EntityBase, IUsesGuidId, IHasReferenceImage<E> // e.g. T is Location
    where E : EntityBase, IUsesGuidId, IEntityReferenceImage, // e.g. E is LocationReferenceMapImage
    new()
{
    private readonly IAzureBlobService _azureBlobService;
    private readonly IReferenceImageRepository<T, E> _imageRepository;
    private readonly static string AzureContainerName = "reference-images";

    public ReferenceImageService(
        IAzureBlobService azureBlobService,
        IReferenceImageRepository<T, E> imageRepository) 
        => (_azureBlobService, _imageRepository) 
        = (azureBlobService, imageRepository);

    #region Public Functions

    public async Task<ReferenceImageDTO?> CreateReferenceImageAsync(Guid tenantId, CreateReferenceImageDTO dto, string userEmail, CancellationToken cancellationToken = default)
    {
        var blobName = await _azureBlobService.CreateBlobAsync(dto.AzureBlobDTO, AzureContainerName);

        var updatedMetaData = dto.MetaData with { AzureBlobName = blobName, UploadedByCardHolderEmail = userEmail };
        dto = dto with { MetaData = updatedMetaData };

        var entity = CreateReferenceImageMapper<E>.ToEntity(dto);

        var newEntity = await _imageRepository.CreateReferenceImageAsync(tenantId, dto.EntityId, entity, cancellationToken);
        
        return newEntity is null ? null : ReferenceImageMapper<E>.ToDTO(newEntity);
    }

    public async Task<ReferenceImageDTO?> GetReferenceImageAsync(Guid tenantId, Guid referenceImageId, CancellationToken cancellationToken = default)
    {
        var entity = await _imageRepository.GetReferenceImageAsync(tenantId, referenceImageId, cancellationToken) ?? throw new NotFoundException(referenceImageId, "Reference Image");
        
        if (entity is not null && entity.MetaData.AzureBlobName is not null)
        {
            var blobSASToken = _azureBlobService.GetBlobSasUri(entity.MetaData.AzureBlobName, AzureContainerName);

            return ReferenceImageMapper<E>.ToDTOWithSASToken(entity, blobSASToken);
        }

        return entity is null ? null : ReferenceImageMapper<E>.ToDTO(entity);
    }

    public async Task<ReferenceImageDTO?> GetReferenceImageByOwnerIdAsync(Guid tenantId, Guid entityId, CancellationToken cancellationToken = default)
    {
        var entity = await _imageRepository.GetReferenceImageByOwnerIdAsync(tenantId, entityId, cancellationToken);

        if (entity is not null && entity.MetaData.AzureBlobName is not null)
        {
            var blobSASToken = _azureBlobService.GetBlobSasUri(entity.MetaData.AzureBlobName, AzureContainerName);

            return ReferenceImageMapper<E>.ToDTOWithSASToken(entity, blobSASToken);
        }

        return entity is null ? null : ReferenceImageMapper<E>.ToDTO(entity);
    }

    public Task UpdateReferenceImageAsync(Guid tenantId, Guid referenceImageId, UpdateReferenceImageDTO dto, CancellationToken cancellationToken = default)
    {
        throw new  NotImplementedException();
    }

    public async Task DeleteReferenceImageAsync(Guid tenantId, Guid referenceImageId, CancellationToken cancellationToken = default)
    {
        var entity = await _imageRepository.GetReferenceImageAsync(tenantId, referenceImageId, cancellationToken);

        if (entity is not null)
        {
            await _imageRepository.DeleteReferenceImageAsync(tenantId, referenceImageId, cancellationToken);

            if (entity.MetaData.AzureBlobName is not null)
            {
                await _azureBlobService.DeleteBlobAsync(entity.MetaData.AzureBlobName, AzureContainerName);
            }
        }
    }

    #endregion Public Functions
}
