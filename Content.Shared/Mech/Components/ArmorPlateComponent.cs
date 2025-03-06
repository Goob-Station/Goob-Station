using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Content.Shared.Damage.Prototypes;

namespace Content.Shared.Mech.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class ArmorPlateComponent : Component
{
    [DataField("damageModifierSet")]
    public ProtoId<DamageModifierSetPrototype>? DamageModifierSetId;
}
