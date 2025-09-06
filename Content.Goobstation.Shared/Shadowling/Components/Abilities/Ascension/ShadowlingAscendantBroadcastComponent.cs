using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Shadowling.Components.Abilities.Ascension;

/// <summary>
/// This is used for Ascendant Broadcast ability.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingAscendantBroadcastComponent : Component
{
    [DataField]
    public EntProtoId ActionId = "ActionAscendantBroadcast";

    [DataField]
    public EntityUid? ActionEnt;
}
