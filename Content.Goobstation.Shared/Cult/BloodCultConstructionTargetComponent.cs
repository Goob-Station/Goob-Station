using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Cult;

[RegisterComponent, NetworkedComponent]
public sealed partial class BloodCultConstructionTargetComponent : Component
{
    [DataField(required: true)] public EntProtoId Result;
}
