using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared._White.Actions;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PlasmaCostActionComponent : Component
{
    [DataField]
    public bool ShouldChangePlasma = true;

    [DataField, AutoNetworkedField]
    public FixedPoint2 PlasmaCost = 50;
}
