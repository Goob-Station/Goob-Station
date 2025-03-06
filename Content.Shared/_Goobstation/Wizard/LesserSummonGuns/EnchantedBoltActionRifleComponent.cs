using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Wizard.LesserSummonGuns;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class EnchantedBoltActionRifleComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public EntityUid? Caster;

    [DataField, AutoNetworkedField]
    public int Shots = 30;

    [DataField]
    public EntProtoId Proto = "WeaponBoltActionEnchanted";

    [DataField]
    public Vector2 ThrowingSpeed = new(2f, 4f);
}
