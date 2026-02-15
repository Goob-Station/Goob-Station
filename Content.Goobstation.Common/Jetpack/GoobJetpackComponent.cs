using Robust.Shared.GameStates;

namespace Content.Goobstation.Common.Jetpack;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GoobJetpackComponent : Component
{
    [DataField, AutoNetworkedField]
    public float HandScatterAngle = 1.5708f;

    [DataField, AutoNetworkedField]
    public float HandScatterFrequency = 0.05f;

    [DataField, AutoNetworkedField]
    public float HandScatterWobble = 0.3f;

    [DataField, AutoNetworkedField]
    public float HandJitterAmplitude = 1.5f;

    [DataField, AutoNetworkedField]
    public float HandJitterFrequency = 3f;
}
