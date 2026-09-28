using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Xenomorph;

/// <summary>
/// Lets this entity see larva infection status icons.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class LarvaInfestationVisionComponent : Component;
