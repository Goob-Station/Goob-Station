using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Jiggle;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class JiggleComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Stiffness = 120f;

    [DataField, AutoNetworkedField]
    public float Damping = 6f;

    [DataField, AutoNetworkedField]
    public float Inertia = 5f;

    [DataField, AutoNetworkedField]
    public float Drag = 0.25f;

    [DataField, AutoNetworkedField]
    public float MaxOffset = 3f;

    [DataField, AutoNetworkedField]
    public float MaxSpeed = 40f;

    [DataField, AutoNetworkedField]
    public float AnimationScale = 1f;

    [DataField, AutoNetworkedField]
    public float SwayAmplitude = 1f;

    [DataField, AutoNetworkedField]
    public float SwayPeriod = 3f;

    [DataField, AutoNetworkedField]
    public float Base = -16f;

    [DataField, AutoNetworkedField]
    public float Height = 32f;
}
