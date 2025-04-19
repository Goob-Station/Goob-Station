using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Spy;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SpyUplinkComponent : Component
{
    public SpyBountyData? ClaimedBounty = null;
}
