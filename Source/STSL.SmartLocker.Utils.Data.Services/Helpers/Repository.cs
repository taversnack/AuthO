using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using STSL.SmartLocker.Utils.Common.Exceptions;
using STSL.SmartLocker.Utils.Data.Contexts;
using STSL.SmartLocker.Utils.Data.Extensions;
using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.Domain;
using System.Data.Common;
using System.Linq.Expressions;

namespace STSL.SmartLocker.Utils.Data.Services.Helpers;

/// <summary>
/// A generic repository class that simplifies common entity database operations 
/// </summary>
/// <typeparam name="TEntity"></typeparam>
internal sealed class Repository<TEntity> : IRepository<TEntity> where TEntity : EntityBase, IUsesGuidId
{
    private readonly DbSet<TEntity> _entities;
    private readonly SmartLockerDbContext _dbContext;
    private readonly ILogger<Repository<TEntity>> _logger;
    private readonly IDatabaseExceptionHandler _exceptionHandler;

    public Repository(SmartLockerDbContext dbContext, IDatabaseExceptionHandler exceptionHandler, ILogger<Repository<TEntity>> logger) =>
        (_dbContext, _entities, _exceptionHandler, _logger) = (dbContext, dbContext.Set<TEntity>(), exceptionHandler, logger);

    #region Create

    public async Task<TEntity?> CreateOneAsync(Guid tenantId, TEntity entity, CancellationToken cancellationToken = default)
    {
        try
        {
            entity.TenantId = tenantId;

            var trackedEntity = _entities.Add(entity);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return trackedEntity.Entity;
        }
        catch (Exception ex)
        {
            HandleExceptions(ex);
            return null;
        }

    }

    public async Task<TEntity?> TryCreateOneAsync(Guid tenantId, TEntity entity, CancellationToken cancellationToken = default)
    {
        try
        {
            return await CreateOneAsync(tenantId, entity, cancellationToken);
        }
        catch (DbException ex)
        {
            _logger.LogError(exception: ex, message: "Failed to create entity of type {EntityTypeName}", typeof(TEntity).Name);
            return null;
        }
    }

    public async Task<bool> CreateManyAsync(Guid tenantId, IReadOnlyCollection<TEntity> entities, CancellationToken cancellationToken = default)
    {
        try
        {
            foreach (var entity in entities)
            {
                entity.TenantId = tenantId;
            }

            _entities.AddRange(entities);

            var count = await _dbContext.SaveChangesAsync(cancellationToken);

            return count == entities.Count;
        }
        catch (Exception ex)
        {
            HandleExceptions(ex);
            return false;
        }

    }

    public async Task<bool> TryCreateManyAsync(Guid tenantId, IReadOnlyCollection<TEntity> entities, CancellationToken cancellationToken = default)
    {
        try
        {
            return await CreateManyAsync(tenantId, entities, cancellationToken);
        }
        catch (DbException ex)
        {
            _logger.LogError(exception: ex, message: "Failed to create entities of type {EntityTypeName}", typeof(TEntity).Name);
            return false;
        }
    }

    #endregion Create

    #region Read

    public async Task<TEntity?> GetOneUntrackedAsync(Guid tenantId, Guid entityId, CancellationToken cancellationToken = default)
        => await _entities
            .AsNoTracking()
            .ByTenant(tenantId)
            .Where(x => x.Id == entityId)
            .FirstOrDefaultAsync(cancellationToken);

    public IQueryable<TEntity> QueryingAllUntracked(Guid tenantId)
        => _entities.AsNoTracking().ByTenant(tenantId);

    public async Task<IReadOnlyList<TEntity>> GetAllUntrackedAsListAsync(Guid tenantId, CancellationToken cancellationToken = default)
        => await QueryingAllUntracked(tenantId).ToListAsync(cancellationToken);

    public async Task<TEntity?> GetOneAsync(Guid tenantId, Guid entityId, CancellationToken cancellationToken = default)
    {
        var entity = await _entities.FindAsync(new object[] { entityId }, cancellationToken: cancellationToken);
        return entity?.TenantId == tenantId ? entity : null;
    }

    public IQueryable<TEntity> QueryingAll(Guid tenantId) => _entities.ByTenant(tenantId);

    public async Task<IReadOnlyList<TEntity>> GetAllAsListAsync(Guid tenantId, CancellationToken cancellationToken = default)
        => await QueryingAll(tenantId).ToListAsync(cancellationToken);

    public bool Exists(Guid tenantId, Func<TEntity, bool> predicate)
        => QueryingAll(tenantId).FirstOrDefault(predicate) is not null;

    public async Task<bool> ExistsAsync(Guid tenantId, Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => await QueryingAll(tenantId).FirstOrDefaultAsync(predicate, cancellationToken) is not null;

    public async Task<bool> ExistsAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
        => (await _entities.FindAsync(new object[] { id }, cancellationToken: cancellationToken))?.TenantId == tenantId;

    #endregion Read

    #region Update

    public async Task UpdateOneAsync(Guid tenantId, Guid id, TEntity entity, CancellationToken cancellationToken = default)
    {
        var existing = await GetOneAsync(tenantId, id, cancellationToken) ?? throw new NotFoundException(id);

        entity.Id = id;
        entity.TenantId = tenantId;
        _dbContext.Entry(existing).CurrentValues.SetValues(entity);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            HandleExceptions(ex);
        }
    }

    public async Task<bool> TryUpdateOneAsync(Guid tenantId, Guid id, TEntity entity, CancellationToken cancellationToken = default)
    {
        try
        {
            await UpdateOneAsync(tenantId, id, entity, cancellationToken);
            return true;
        }
        catch (DbException ex)
        {
            _logger.LogError(exception: ex, message: "Failed to update entity of type {EntityTypeName}", typeof(TEntity).Name);
            return false;
        }
    }

    public async Task<int> UpdateManyAsync(Guid tenantId, IReadOnlyList<TEntity> entities, CancellationToken cancellationToken = default)
    {
        var existing = await QueryingAll(tenantId)
            .Where(x => entities.Select(y => y.Id).Contains(x.Id))
            .ToListAsync();

        foreach (var entity in existing)
        {
            entity.TenantId = tenantId;
            _dbContext.Entry(existing).CurrentValues.SetValues(entity);
        }

        try
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            HandleExceptions(ex);
            return 0;
        }
    }

    #endregion Update

    #region Delete

    public async Task<TEntity?> DeleteOneAsync(Guid tenantId, Guid entityId, bool throwIfNotFound = false, CancellationToken cancellationToken = default)
    {
        var entity = await GetOneAsync(tenantId, entityId, cancellationToken);

        if (entity is null)
        {
            if (throwIfNotFound)
            {
                throw new NotFoundException(entityId);
            }
            return null;
        }

        try
        {
            _entities.Remove(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return entity;
        }
        catch (Exception ex)
        {
            HandleExceptions(ex);
            return null;
        }
    }

    public async Task<TEntity?> TryDeleteOneAsync(Guid tenantId, Guid entityId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await DeleteOneAsync(tenantId, entityId, false, cancellationToken);
        }
        catch (DbException ex)
        {
            _logger.LogError(exception: ex, message: "Failed to delete entity of type {EntityTypeName}", typeof(TEntity).Name);
            return null;
        }
    }

    #endregion Delete

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _dbContext.SaveChangesAsync(cancellationToken);

    #region Validate

    public void ValidateOne(TEntity entity, IValidator<TEntity> validator)
        => ValidateOne(entity, validator, typeof(TEntity).Name);

    public void ValidateOne(TEntity entity, IValidator<TEntity> validator, string entityName)
    {
        var validationResult = validator.Validate(entity);

        if (validationResult.IsInvalid)
        {
            throw new BadRequestException($"{entityName} invalid", validationResult.ErrorsDictionary);
        }
    }

    public void ValidateMany(IReadOnlyList<TEntity> entities, IValidator<TEntity> validator, bool throwOnFirstError = false)
        => ValidateMany(entities, validator, typeof(TEntity).Name, throwOnFirstError);

    public void ValidateMany(IReadOnlyList<TEntity> entities, IValidator<TEntity> validator, string entityName, bool throwOnFirstError = false)
    {
        var validationErrors = ValidationResult.CreateIndexedResults();
        foreach (var (entity, index) in entities.Select((entity, index) => (entity, index)))
        {
            var validationResult = validator.Validate(entity);
            if (validationResult.IsInvalid)
            {
                if (throwOnFirstError)
                {
                    throw new BadRequestException($"{entityName} invalid", validationResult.ErrorsDictionary);
                }
                else
                {
                    validationErrors.TryAdd(index, validationResult.ErrorsDictionary);
                }
            }
        }
        if (validationErrors.Any())
        {
            throw new BadRequestException($"There is one or more invalid {entityName}", validationErrors);
        }
    }

    #endregion Validate

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
}
