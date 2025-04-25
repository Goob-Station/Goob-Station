using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Spy;

/// <summary>
/// This handles...
/// </summary>
public abstract partial class SharedSpySystem : EntitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
    }
}
