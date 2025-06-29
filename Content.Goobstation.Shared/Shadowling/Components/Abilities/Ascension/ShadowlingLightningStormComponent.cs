using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Shadowling.Components.Abilities.Ascension;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingLightningStormComponent : Component
{
    [DataField]
    public TimeSpan TimeBeforeActivation = TimeSpan.FromSeconds(10);

    [DataField]
    public float Range = 12f;

    [DataField]
    public int BoltCount = 15;

    [DataField]
    public EntProtoId LightningProto = "HyperchargedLightning";
}
