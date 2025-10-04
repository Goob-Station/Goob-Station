using Content.Shared.Damage;
using Content.Shared.Mobs;
using Content.Shared.Module;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Revenant;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class WraithRevenantComponent : Component
{
    [ViewVariables]
    public EntProtoId RevenantAbilities = "RevenantAbilities";

    [DataField, AutoNetworkedField]
    public DamageSpecifier DamageOvertime;

    [DataField, AutoNetworkedField]
    public List<MobState> AllowedStates = new();

    [ViewVariables, AutoNetworkedField]
    public DamageSpecifier? OldDamageSpecifier;

    [ViewVariables, AutoNetworkedField]
    public bool HadPassive;
}
