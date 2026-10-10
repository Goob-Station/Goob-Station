using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Mech.Components;

/// <summary>
/// Only allows the mech to fire within this many degrees of its facing direction.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MechFiringArcComponent : Component
{
    [DataField, AutoNetworkedField]
    public float ArcDegrees = 45f;
}
