using Content.Shared.Weapons.Ranged;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Wizard.Mutate;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BasicHitscanAmmoProviderComponent : AmmoProviderComponent
{
    [ViewVariables(VVAccess.ReadWrite), DataField(required: true), AutoNetworkedField]
    public ProtoId<HitscanPrototype> Proto;
}
