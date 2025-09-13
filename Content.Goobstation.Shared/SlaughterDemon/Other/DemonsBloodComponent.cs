using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.SlaughterDemon.Other;

/// <summary>
/// Makes you unable to be consumed by the slaughter demon
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class DemonsBloodComponent : Component;
