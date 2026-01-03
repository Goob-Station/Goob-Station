using Content.Server.Database;
using Microsoft.EntityFrameworkCore;

namespace Content.Goobstation.Server.Database;

public sealed class GoobstationServerDbContext : ServerDbContext
{
    public GoobstationServerDbContext(DbContextOptions<GoobstationServerDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        base.OnConfiguring(options);

        // Goobstation-specific configuration here
    }

    // ass
    public override int CountAdminLogs()
    {
        return AdminLog.Count();
    }
}
