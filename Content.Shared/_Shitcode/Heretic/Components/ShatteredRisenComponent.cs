using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShatteredRisenComponent : Component
{
    [DataField]
    public EntProtoId Weapon1 = "ArmBladeShattered";

    [DataField]
    public EntProtoId Weapon2 = "ArmHammerShattered";
}
