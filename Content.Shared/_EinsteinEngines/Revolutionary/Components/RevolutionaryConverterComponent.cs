using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._EinsteinEngines.Revolutionary.Components;

[Serializable, NetSerializable]
public sealed partial class RevolutionaryConverterDoAfterEvent : SimpleDoAfterEvent
{
}

[RegisterComponent, NetworkedComponent]
public sealed partial class RevolutionaryConverterComponent : Component
{
    [DataField]
    public TimeSpan ConversionDuration { get; set; }

    [DataField]
    public bool Silent { get; set; }
}
