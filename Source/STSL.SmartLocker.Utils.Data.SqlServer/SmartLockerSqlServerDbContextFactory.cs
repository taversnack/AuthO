using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using STSL.SmartLocker.Utils.Data.Contexts;
using STSL.SmartLocker.Utils.Data.SqlServer.Contexts;
using System.Reflection;

namespace STSL.SmartLocker.Utils.Data.SqlServer;

// Design time context creation
internal class SmartLockerSqlServerDbContextFactory : IDesignTimeDbContextFactory<SmartLockerSqlServerDbContext>
{
    public SmartLockerSqlServerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SmartLockerDbContext>();
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=STSL.SmartLocker;Trusted_Connection=True;", options =>
        {
            
            options.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
        });

        return new SmartLockerSqlServerDbContext(optionsBuilder.Options);
    }
}