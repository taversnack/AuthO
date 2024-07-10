using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using STSL.SmartLocker.Utils.Data.SqlServer.StoredProcedures.Results;

namespace STSL.SmartLocker.Utils.Data.SqlServer.EntityConfiguration;

internal sealed class ProcessMSIData_Result_Configuration : IEntityTypeConfiguration<ProcessMSIData_Result>
{
    public void Configure(EntityTypeBuilder<ProcessMSIData_Result> builder)
    {
        builder.Metadata.SetIsTableExcludedFromMigrations(true);
        builder.HasKey(e => e.ObjectID);
    }
}

