using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Slasher.Components;

/// <summary>
/// Applied to an entity to block gun usage (shoot attempts are cancelled).
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SlasherGunBlockedComponent : Component
{
}
