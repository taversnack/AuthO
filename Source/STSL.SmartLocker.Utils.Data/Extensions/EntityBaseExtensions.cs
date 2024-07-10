using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using STSL.SmartLocker.Utils.Domain;

namespace STSL.SmartLocker.Utils.Data.Extensions;

public static class EntityBaseExtensions
{
    public static IQueryable<T> ByTenant<T>(this IQueryable<T> query, Guid tenantId) where T : EntityBase => query.Where(x => x.TenantId == tenantId);

    public static IQueryable<T> ByTenants<T>(this IQueryable<T> query, IEnumerable<Guid> tenants) where T : EntityBase => query.Where(x => tenants.Contains(x.TenantId));

    public static IQueryable<T> IgnoreDeleted<T>(this IQueryable<T> query) where T : EntityBase, IUsesSoftDelete => query.Where(x => !x.IsDeleted);

    public static void SoftDelete<T>(this T entity) where T : EntityBase, IUsesSoftDelete => entity.IsDeleted = true;

    internal static void UpdateAuditableEntitiesOnSave(this ChangeTracker changeTracker)
    {
        changeTracker.DetectChanges();

        foreach (var entry in changeTracker.Entries())
        {
            if (entry.Entity is IUsesAuditing auditableEntity)
            {
                if (entry.State == EntityState.Added)
                {
                    auditableEntity.CreatedAt = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    auditableEntity.LastModifiedAt = DateTime.UtcNow;
                }
            }
        }
    }
}