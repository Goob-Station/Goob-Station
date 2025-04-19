using System.Diagnostics.CodeAnalysis;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.Spy;

/// <summary>
/// This handles...
/// </summary>
public abstract partial class SharedSpyBountySystem : EntitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] private readonly INetManager _net = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {

    }

    public virtual void SetupBounties() { }
    public virtual void CreateDbEntity() { }
    protected virtual bool TryGetSpyDatabaseEntity([NotNullWhen(true)] out Entity<SpyBountyDatabaseComponent>? entity)
    {
        entity = null;
        return false;
    }
}
