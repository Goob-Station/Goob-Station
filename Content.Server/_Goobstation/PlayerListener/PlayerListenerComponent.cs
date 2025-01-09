using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Server._Goobstation.PlayerListener;

/// <summary>
///     Stores data about players, listens even.
/// </summary>
[RegisterComponent]
public sealed partial class PlayerListenerComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public readonly HashSet<NetUserId> UserIds = [];
}
