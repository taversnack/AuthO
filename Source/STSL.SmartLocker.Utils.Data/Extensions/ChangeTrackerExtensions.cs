using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using STSL.SmartLocker.Utils.Domain;

namespace STSL.SmartLocker.Utils.Data.Extensions;

internal static class ChangeTrackerExtensions
{
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
