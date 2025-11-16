using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Voodoo;

/// <summary>
/// Component Used to track people with a certain name for the voodoo system
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class VoodooComponent : Component
{
    [DataField, AutoNetworkedField]
    public string TargetName;

    [DataField, AutoNetworkedField]
    public bool GibOnDestory;

    [AutoNetworkedField]
    public ProtoId<DamageGroupPrototype> DamageType = "Brute";

    [DataField, AutoNetworkedField]
    public float Damage = 10f;

    [DataField, AutoNetworkedField]
    public float DamageOnDestroy = 200f;
}
