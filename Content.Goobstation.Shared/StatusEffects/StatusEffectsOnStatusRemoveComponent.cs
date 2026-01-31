using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.StatusEffects;

[RegisterComponent, NetworkedComponent]
public sealed partial class StatusEffectsOnStatusRemoveComponent : Component
{
    [DataField(required: true)]
    public Dictionary<EntProtoId, TimeSpan> StatusEffects;
}
