using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using STSL.SmartLocker.Utils.Data.SqlServer.StoredProcedures.Results;

namespace STSL.SmartLocker.Utils.Data.SqlServer.EntityConfiguration;

internal sealed class ListAuditRecordsForLocker_Result_Configuration : IEntityTypeConfiguration<ListAuditRecordsForLocker_Result>
{
    public void Configure(EntityTypeBuilder<ListAuditRecordsForLocker_Result> builder)
    {
        builder.Metadata.SetIsTableExcludedFromMigrations(true);
        builder.HasKey(e => e.RowNum);
    }
}

