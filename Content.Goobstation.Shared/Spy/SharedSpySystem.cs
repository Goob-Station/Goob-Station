using Robust.Shared.Network;

namespace Content.Goobstation.Shared.Spy;

/// <summary>
/// This handles...
/// </summary>
public abstract partial class SharedSpySystem : EntitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] private readonly INetManager _net = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
    }
}
