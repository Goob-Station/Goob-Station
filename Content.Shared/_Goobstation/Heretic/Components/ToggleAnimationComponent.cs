using System.Threading;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class ToggleAnimationComponent : Component
{
    [DataField]
    public TimeSpan ToggleOnTime = TimeSpan.FromSeconds(2);

    [DataField]
    public TimeSpan ToggleOffTime = TimeSpan.FromSeconds(1.6);

    public CancellationTokenSource? TokenSource;
}

[Serializable, NetSerializable]
public enum ToggleAnimationVisuals : byte
{
    ToggleState,
}

[Serializable, NetSerializable]
public enum ToggleAnimationState : byte
{
    Off,
    TogglingOn,
    On,
    TogglingOff,
}
