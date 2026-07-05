using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.MalfunctionAi;

/// <summary>
/// Marks an APC that has already been hacked by a Malfunction AI, so it cannot be
/// hacked again for more processing power.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MalfHackedApcComponent : Component;
