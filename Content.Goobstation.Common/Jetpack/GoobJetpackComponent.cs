using Robust.Shared.GameStates;

namespace Content.Goobstation.Common.Jetpack;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GoobJetpackComponent : Component
{
    [DataField, AutoNetworkedField]
    public int HandDirectionHoldTicks = 40;

    [DataField, AutoNetworkedField]
    public float HandScatterWobble = 0.2f;

    [DataField, AutoNetworkedField]
    public float HandJitterAmplitude = 3f;

    [DataField, AutoNetworkedField]
    public float HandJitterFrequency = 5f;
}
