using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Revenant;

[RegisterComponent, NetworkedComponent]
public sealed partial class WraithRevenantComponent : Component
{
    [ViewVariables]
    public EntProtoId RevenantAbilities = "RevenantAbilities";
}
