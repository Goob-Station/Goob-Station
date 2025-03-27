using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Changeling;

[RegisterComponent, NetworkedComponent]
public sealed partial class ChangelingChemicalsAmmoProviderComponent : AmmoProviderComponent
{
    [DataField]
    public float FireCost = 7f;

    [ViewVariables(VVAccess.ReadWrite), DataField(required: true)]
    public EntProtoId Proto;
}
