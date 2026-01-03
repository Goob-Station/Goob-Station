using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Content.Goobstation.Server.Database;

public abstract class GoobstationServerDbContext : DbContext
{
    public DbSet<BrainrotWord> BrainrotWords { get; set; } = null!;

    protected GoobstationServerDbContext(DbContextOptions options) : base(options)
    {
    }
}

public sealed class GoobstationSqliteServerDbContext : GoobstationServerDbContext
{
    public GoobstationSqliteServerDbContext(DbContextOptions<GoobstationSqliteServerDbContext> options)
        : base(options)
    {
    }
}

public sealed class GoobstationPostgresServerDbContext : GoobstationServerDbContext
{
    public GoobstationPostgresServerDbContext(DbContextOptions<GoobstationPostgresServerDbContext> options)
        : base(options)
    {
    }
}
