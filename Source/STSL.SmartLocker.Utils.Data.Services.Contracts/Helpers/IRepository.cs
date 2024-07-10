using System.Linq.Expressions;

namespace STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;

public interface IRepository<TEntity>
{
    Task<TEntity?> CreateOneAsync(Guid tenantId, TEntity entity, CancellationToken cancellationToken = default);
    Task<TEntity?> TryCreateOneAsync(Guid tenantId, TEntity entity, CancellationToken cancellationToken = default);

    Task<bool> CreateManyAsync(Guid tenantId, IReadOnlyCollection<TEntity> entities, CancellationToken cancellationToken = default);
    Task<bool> TryCreateManyAsync(Guid tenantId, IReadOnlyCollection<TEntity> entities, CancellationToken cancellationToken = default);

    Task<TEntity?> GetOneUntrackedAsync(Guid tenantId, Guid entityId, CancellationToken cancellationToken = default);
    IQueryable<TEntity> QueryingAllUntracked(Guid tenantId);
    Task<IReadOnlyList<TEntity>> GetAllUntrackedAsListAsync(Guid tenantId, CancellationToken cancellationToken = default);


    Task<TEntity?> GetOneAsync(Guid tenantId, Guid entityId, CancellationToken cancellationToken = default);
    IQueryable<TEntity> QueryingAll(Guid tenantId);
    Task<IReadOnlyList<TEntity>> GetAllAsListAsync(Guid tenantId, CancellationToken cancellationToken = default);

    bool Exists(Guid tenantId, Func<TEntity, bool> predicate);
    Task<bool> ExistsAsync(Guid tenantId, Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default);

    Task UpdateOneAsync(Guid tenantId, Guid id, TEntity entity, CancellationToken cancellationToken = default);
    Task<bool> TryUpdateOneAsync(Guid tenantId, Guid id, TEntity entity, CancellationToken cancellationToken = default);

    Task<int> UpdateManyAsync(Guid tenantId, IReadOnlyList<TEntity> entities, CancellationToken cancellationToken = default);
    //Task<bool> TryUpdateManyAsync(Guid tenantId, IReadOnlyList<TEntity> entities, CancellationToken cancellationToken = default);

    Task<TEntity?> DeleteOneAsync(Guid tenantId, Guid entityId, bool throwIfNotFound = false, CancellationToken cancellationToken = default);
    Task<TEntity?> TryDeleteOneAsync(Guid tenantId, Guid entityId, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    void ValidateOne(TEntity entity, IValidator<TEntity> validator);
    void ValidateOne(TEntity entity, IValidator<TEntity> validator, string entityName);
    //bool TryValidateOne(TEntity entity, IValidator<TEntity> validator);
    //bool TryValidateOne(TEntity entity, IValidator<TEntity> validator, string entityName);

    void ValidateMany(IReadOnlyList<TEntity> entities, IValidator<TEntity> validator, bool throwOnFirstError = false);
    void ValidateMany(IReadOnlyList<TEntity> entities, IValidator<TEntity> validator, string entityName, bool throwOnFirstError = false);
    //bool TryValidateMany(IReadOnlyList<TEntity> entities, IValidator<TEntity> validator);
    //bool TryValidateMany(IReadOnlyList<TEntity> entities, IValidator<TEntity> validator, string entityName);
}
