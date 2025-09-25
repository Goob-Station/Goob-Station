using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Shadowling.Components.Abilities.Ascension;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingLightningStormComponent : Component
{
    [DataField]
    public TimeSpan TimeBeforeActivation = TimeSpan.FromSeconds(10);

    /// <summary>
    /// The search radius of this ability.
    /// </summary>
    [DataField]
    public float Range = 12f;

    /// <summary>
    /// The amount of lightning bolts it fires from the user
    /// </summary>
    [DataField]
    public int BoltCount = 15;

    /// <summary>
    /// The prototype effect that the lightning bolt has
    /// </summary>
    [DataField]
    public EntProtoId LightningProto = "HyperchargedLightning";

    [DataField]
    public EntProtoId ActionId = "ActionLightningStorm";

    [DataField]
    public EntityUid? ActionEnt;
}
