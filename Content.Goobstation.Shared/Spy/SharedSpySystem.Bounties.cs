using System.Diagnostics.CodeAnalysis;

namespace Content.Goobstation.Shared.Spy;

public abstract partial class SharedSpySystem
{
    public virtual void SetupBounties() { }
    public virtual void CreateDbEntity() { }
    protected virtual bool TryGetSpyDatabaseEntity([NotNullWhen(true)] out Entity<SpyBountyDatabaseComponent>? entity)
    {
        entity = null;
        return false;
    }
}
