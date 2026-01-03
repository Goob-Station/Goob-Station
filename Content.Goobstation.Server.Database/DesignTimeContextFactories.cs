#if TOOLS

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SQLitePCL;

namespace Content.Goobstation.Server.Database;

public sealed class GoobstationDesignTimeContextFactoryPostgres : IDesignTimeDbContextFactory<GoobstationPostgresServerDbContext>
{
    public GoobstationPostgresServerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<GoobstationPostgresServerDbContext>();
        optionsBuilder.UseNpgsql("Server=localhost");
        return new GoobstationPostgresServerDbContext(optionsBuilder.Options);
    }
}

public sealed class GoobstationDesignTimeContextFactorySqlite : IDesignTimeDbContextFactory<GoobstationSqliteServerDbContext>
{
    public GoobstationSqliteServerDbContext CreateDbContext(string[] args)
    {
#if !USE_SYSTEM_SQLITE
        raw.SetProvider(new SQLite3Provider_e_sqlite3());
#endif

        var optionsBuilder = new DbContextOptionsBuilder<GoobstationSqliteServerDbContext>();
        optionsBuilder.UseSqlite("Data Source=:memory:");
        return new GoobstationSqliteServerDbContext(optionsBuilder.Options);
    }
}

#endif
