using Content.Shared.DoAfter;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Mech.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MechTurningComponent : Component
{
    [DataField, AutoNetworkedField]
    public float SnapArcDegrees = 45f;

    [DataField, AutoNetworkedField]
    public TimeSpan ShortTurnDelay = TimeSpan.FromSeconds(0.75);

    [DataField, AutoNetworkedField]
    public TimeSpan FullTurnDelay = TimeSpan.FromSeconds(1.5);

    [ViewVariables, AutoNetworkedField]
    public bool IsTurning;

    [ViewVariables, AutoNetworkedField]
    public Angle TargetRotation;

    [ViewVariables]
    public DoAfterId? DoAfter;

    [DataField]
    public TimeSpan PollInterval = TimeSpan.FromSeconds(0.1);

    [DataField]
    public TimeSpan ResendInterval = TimeSpan.FromSeconds(1);

    [ViewVariables]
    public TimeSpan NextPoll;

    [ViewVariables]
    public TimeSpan LastSend;

    [ViewVariables]
    public Direction? LastRequested;
}
