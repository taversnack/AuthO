using Microsoft.EntityFrameworkCore;
using STSL.SmartLocker.Utils.Data.Contexts;
using STSL.SmartLocker.Utils.Data.SqlServer.StoredProcedures.Results;
using STSL.SmartLocker.Utils.Data.SqlServer.Views;

namespace STSL.SmartLocker.Utils.Data.SqlServer.Contexts;

public sealed class SmartLockerSqlServerDbContext : SmartLockerDbContext
{
    public SmartLockerSqlServerDbContext(DbContextOptions<SmartLockerDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SmartLockerSqlServerDbContext).Assembly);
    }

    // View and Stored Procedure DbSets (non-materialised)
    public DbSet<ListAuditRecordsForLocker_Result> ListAuditRecordsForLocker_Results { get; set; }
    public DbSet<LockersWithStatus_View> LockersWithStatus_View { get; set; }
    public DbSet<ProcessMSIData_Result> ProcessMSIOutput_Results { get; set; }
    public DbSet<LockerBankAdminSummaries_View> LockerBankSummaries_View { get; set; }
}
