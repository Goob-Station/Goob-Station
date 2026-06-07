using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Revenant;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class WraithRevenantComponent : Component
{
    [ViewVariables]
    public EntProtoId RevenantAbilities = "RevenantAbilities";

    [ViewVariables, AutoNetworkedField]
    public DamageSpecifier? OldDamageSpecifier;

    [ViewVariables, AutoNetworkedField]
    public bool HadPassive;
}
