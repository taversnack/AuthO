using Microsoft.EntityFrameworkCore;
using STSL.SmartLocker.Utils.Common.Exceptions;
using STSL.SmartLocker.Utils.Data.Contexts;
using STSL.SmartLocker.Utils.Data.Extensions;
using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.Domain;
using System.Data.Common;

namespace STSL.SmartLocker.Utils.Data.Services.Helpers;

internal sealed class ReferenceImageRepository<T, E> : IReferenceImageRepository<T, E> 
    where T : EntityBase, IUsesGuidId, IHasReferenceImage<E> 
    where E : EntityBase, IUsesGuidId, IEntityReferenceImage
{
    private readonly DbSet<E> _images;
    private readonly DbSet<T> _entities;
    private readonly SmartLockerDbContext _dbContext;
    private readonly IDatabaseExceptionHandler _exceptionHandler;
    
    public ReferenceImageRepository(SmartLockerDbContext dbContext, IDatabaseExceptionHandler exceptionHandler)
        => (_dbContext, _images, _entities, _exceptionHandler) 
        = (dbContext, dbContext.Set<E>(), dbContext.Set<T>(), exceptionHandler);

    #region Images
    public async Task<E?> CreateReferenceImageAsync(Guid tenantId, Guid entityId, E image, CancellationToken cancellationToken)
    {
        try
        {
            image.TenantId = tenantId;

            var trackedImage = _images.Add(image);

            await UpdateEntityWithCurrentReferenceImageAsync(tenantId, trackedImage.Entity.Id, entityId, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return trackedImage.Entity;
        }
        catch (Exception ex)
        {
            HandleExceptions(ex);
            return null;
        }
    }

    public async Task<E?> GetReferenceImageAsync(Guid tenantId, Guid referenceImageId, CancellationToken cancellationToken = default)
    {
        var image = await _images
            .AsNoTracking()
            .ByTenant(tenantId)
            .Where(x => x.Id == referenceImageId)
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException(referenceImageId);

        return image;
    }

    public async Task<E?> GetReferenceImageByOwnerIdAsync(Guid tenantId, Guid entityId, CancellationToken cancellationToken = default)
    {
        var image = await _entities
            .AsNoTracking()
            .ByTenant(tenantId)
            .Where(x => x.Id == entityId)
            .Include(x => x.CurrentReferenceImage)
            .Select(s => s.CurrentReferenceImage)
            .FirstOrDefaultAsync(cancellationToken);

        return image;
    }

    public async Task DeleteReferenceImageAsync(Guid tenantId, Guid referenceImageId, CancellationToken cancellationToken = default)
    {
        var image = await GetImageAsync(tenantId, referenceImageId, cancellationToken);

        try
        {
            _images.Remove(image);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            HandleExceptions(ex);
        }
    }

    public Task<T> UpdateCurrentReferenceImageAsync(T entity)
    {
        throw new NotImplementedException();
    }

    #endregion Images

    #region Private Functions
    private async Task UpdateEntityWithCurrentReferenceImageAsync(Guid tenantId, Guid imageId, Guid entityId, CancellationToken cancellationToken)
    {
        var entity = await GetEntityAsync(tenantId, entityId, cancellationToken);

        entity.CurrentReferenceImageId = imageId;

        try
        {
            _entities.Update(entity);
        }
        catch (Exception ex)
        {
            HandleExceptions(ex);
        }
    }

    private async Task<T> GetEntityAsync(Guid tenantId, Guid entityId, CancellationToken cancellationToken)
    {
        var entity = await _entities
            .AsNoTracking()
            .ByTenant(tenantId)
            .Where(x => x.Id == entityId)
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException(entityId);

        return entity;
    }
    
    private async Task<E> GetImageAsync(Guid tenantId, Guid imageId, CancellationToken cancellationToken)
    {
        var image = await _images
            .AsNoTracking()
            .ByTenant(tenantId)
            .Where(x => x.Id == imageId)
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException(imageId);

        return image;
    }

    private void HandleExceptions(Exception ex)
    {
        if (ex is DbUpdateConcurrencyException)
        {
            throw new ConflictException("The entity you are trying to update has been updated by someone else, please recheck the current value and try again if necessary");
        }
        else if (ex is DbUpdateException updateException && updateException.InnerException is DbException innerException)
        {
            _exceptionHandler.HandleException(innerException);
        }
        else
        {
            throw ex;
        }
    }
    #endregion Private Funtions
}