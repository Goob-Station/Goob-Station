using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared._White.Xenomorphs.Stealth;

[RegisterComponent, NetworkedComponent]
public sealed partial class StealthOnWalkComponent : Component
{
    [DataField]
    public FixedPoint2 PlasmaCost;

    [ViewVariables]
    public bool Stealth;
}
