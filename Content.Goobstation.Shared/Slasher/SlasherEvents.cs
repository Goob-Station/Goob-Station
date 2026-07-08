using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Slasher;

[ByRefEvent]
public sealed partial class SlasherRegenerateEvent : InstantActionEvent;

[ByRefEvent]
public sealed partial class SlasherMassacreEvent : InstantActionEvent;

[ByRefEvent]
public sealed partial class SlasherPossessionEvent : EntityTargetActionEvent;

/// <summary>
/// Toggle event for the blood trail action.
/// </summary>
[ByRefEvent]
public sealed partial class ToggleBloodTrailEvent : InstantActionEvent;

/// <summary>
/// Soul steal targeted action event.
/// </summary>
[ByRefEvent]
public sealed partial class SlasherSoulStealEvent : EntityTargetActionEvent;

[ByRefEvent]
public sealed partial class SlasherStaggerAreaEvent : InstantActionEvent;

[ByRefEvent]
public sealed partial class SlasherSummonMacheteEvent : InstantActionEvent;

[ByRefEvent]
public sealed partial class SlasherSummonMeatSpikeEvent : InstantActionEvent;

[ByRefEvent]
public sealed partial class SlasherRelentlessGrabEvent : InstantActionEvent;

/// <summary>
/// DoAfter event raised when Soul Steal completes.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class SlasherSoulStealDoAfterEvent : SimpleDoAfterEvent;

#region Incorporealize / corporealize events

[Serializable, NetSerializable]
public sealed partial class SlasherIncorporealizeDoAfterEvent : SimpleDoAfterEvent;

[ByRefEvent]
public sealed partial class SlasherIncorporealizeEvent : InstantActionEvent;

[ByRefEvent]
public sealed partial class SlasherCorporealizeEvent : InstantActionEvent;

[ByRefEvent]
public sealed class SlasherIncorporealEnteredEvent : EntityEventArgs;

/// <summary>
/// Event raised to check if any players can see the slasher.
/// </summary>
[Serializable, NetSerializable]
public sealed class SlasherIncorporealObserverCheckEvent(NetEntity slasher, float range) : EntityEventArgs
{
    /// <summary>
    /// The slasher attempting to go incorporeal.
    /// </summary>
    public NetEntity Slasher { get; } = slasher;

    /// <summary>
    /// Range to check for observers with line of sight. Takes number from component.
    /// </summary>
    public float Range { get; } = range;

    /// <summary>
    /// True if the attempt should be cancelled.
    /// </summary>
    public bool Cancelled { get; set; }
}

/// <summary>
/// Event raised to check if any active surveillance cameras can see the slasher.
/// </summary>
[ByRefEvent]
public sealed class SlasherIncorporealCameraCheckEvent(NetEntity slasher, float range) : EntityEventArgs
{
    /// <summary>
    /// The slasher attempting to go incorporeal.
    /// </summary>
    public NetEntity Slasher { get; } = slasher;

    /// <summary>
    /// Range to check for active cameras with line of sight.
    /// </summary>
    public float Range { get; } = range;

    /// <summary>
    /// True if a camera can see the slasher.
    /// </summary>
    public bool Cancelled { get; set; }
}

#endregion
