using Robust.Shared.GameStates;

namespace Content.Goobstation.Common.MomentumSteering;

/// <summary>
/// Makes sure you fly into a wall at high nograv speeds
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MomentumSteeringComponent : Component
{
    [DataField, AutoNetworkedField]
    public float SpeedThreshold = 2.5f;

    [DataField, AutoNetworkedField]
    public float MaxSpeed = 7.5f;

    [DataField, AutoNetworkedField]
    public float MinSteeringFactor = 0.1f;

    [DataField, AutoNetworkedField]
    public float FrictionReductionAtSpeed = 0.3f;

    [DataField, AutoNetworkedField]
    public float BrakingFactor = 0.125f;

    [DataField, AutoNetworkedField]
    public float JitterSpeedThreshold = 5.0f;

    [DataField, AutoNetworkedField]
    public float JitterAmplitude = 2.0f;

    [DataField, AutoNetworkedField]
    public float JitterFrequency = 2.0f;

    [DataField, AutoNetworkedField]
    public TimeSpan LastJitterTime = TimeSpan.Zero;
}
