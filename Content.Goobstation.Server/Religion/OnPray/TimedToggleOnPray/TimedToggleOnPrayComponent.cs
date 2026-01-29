using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Server.Religion.OnPray.TimedToggleOnPray;

[RegisterComponent]
public sealed partial class TimedToggleOnPrayComponent : Component
{
    [DataField]
    public float Duration = 1f;

    [DataField]
    public bool UseDelayOnPray = true;

    [DataField]
    public bool TimerRun = false;

    [DataField]
    public TimeSpan Time;

    [DataField, AutoNetworkedField]
    public bool Activated = false;

    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public bool Predictable = true;

    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public SoundSpecifier? SoundActivate;

    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public SoundSpecifier? SoundDeactivate;
}
