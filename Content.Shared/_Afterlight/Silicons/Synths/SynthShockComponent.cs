using Content.Shared.Damage.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Afterlight.Silicons.Synths;

[RegisterComponent, NetworkedComponent]
public sealed partial class SynthShockComponent : Component
{
    [DataField]
    public ProtoId<DamageTypePrototype> ShockDamageType = "Shock";

    [DataField]
    public float BatteryChargeMultiplier = 2f;

    [DataField]
    public ProtoId<DamageTypePrototype> CellularDamageType = "Cellular";

    [DataField]
    public float CellularDamageMultiplier = 0.2f;
}
