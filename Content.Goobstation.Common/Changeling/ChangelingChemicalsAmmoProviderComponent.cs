using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.ViewVariables;

namespace Content.Goobstation.Common.Changeling;

[RegisterComponent, NetworkedComponent]
public sealed partial class ChangelingChemicalsAmmoProviderComponent : Component
{
    [DataField]
    public float FireCost = 7f;

    [ViewVariables(VVAccess.ReadWrite), DataField(required: true)]
    public EntProtoId Proto;
}
