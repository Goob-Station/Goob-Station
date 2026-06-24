using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._Pirate.Plumbing.Components;

/// <summary>
///     Component for plumbing devices which are updated by the plumbing system.
///     Add this to any entity that should receive <see cref="PlumbingDeviceUpdateEvent"/>.
/// </summary>
[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class PlumbingDeviceComponent : Component
{
    /// <summary>
    ///     If true, this device must be anchored before it will receive updates.
    /// </summary>
    [DataField]
    public bool RequireAnchored = true;

    /// <summary>
    ///     How often reagents are transferred (on an update).
    /// </summary>
    [DataField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(2);

    /// <summary>
    ///     Sound played when a UI button is clicked (toggle, select, etc.).
    /// </summary>
    [DataField]
    public SoundSpecifier ClickSound = new SoundPathSpecifier("/Audio/Machines/machine_switch.ogg");

    /// <summary>
    ///     The next time this device should be updated.
    ///     Automatically adjusted when entity is unpaused.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextUpdateTime;
}

/// <summary>
///     Raised on a plumbing device when it should process/transfer reagents.
///     Subscribe to this event in your system to handle device-specific logic.
/// </summary>
[ByRefEvent]
public readonly struct PlumbingDeviceUpdateEvent(float dt)
{
    /// <summary>
    ///     Time since last update in seconds. Used for time-based calculations (just the reactor heating for now)
    /// </summary>
    public readonly float dt = dt;
}
