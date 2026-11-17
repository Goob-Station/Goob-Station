using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Nullspace;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class NullspacePlayerComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Stamina = 1f;

    [DataField, AutoNetworkedField]
    public bool Exhausted;

    [DataField, AutoNetworkedField]
    public bool Sprinting;

    [DataField, AutoNetworkedField]
    public bool Frozen;

    [DataField, AutoNetworkedField]
    public float Pitch;

    [ViewVariables]
    public Angle Yaw;

    [ViewVariables]
    public Angle PrevYaw;
}
