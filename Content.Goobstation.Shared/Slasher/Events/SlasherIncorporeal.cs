using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Slasher.Events;

[Serializable, NetSerializable]
public sealed partial class SlasherIncorporealizeDoAfterEvent : SimpleDoAfterEvent;

[ByRefEvent]
public sealed partial class SlasherIncorporealizeEvent : InstantActionEvent;

[ByRefEvent]
public sealed partial class SlasherCorporealizeEvent : InstantActionEvent;

[Serializable, NetSerializable]
public sealed class SlasherIncorporealObserverCheckEvent : EntityEventArgs
{
    public SlasherIncorporealObserverCheckEvent(NetEntity slasher, float range)
    {
        Slasher = slasher;
        Range = range;
    }

    /// <summary>
    /// The slasher attempting to go incorporeal.
    /// </summary>
    public NetEntity Slasher { get; }

    /// <summary>
    /// Range to check for observers with line of sight. Takes number from component.
    /// </summary>
    public float Range { get; }

    /// <summary>
    /// True if the attempt should be cancelled.
    /// </summary>
    public bool Cancelled { get; set; }
}

/// <summary>
/// Event raised to check if any active surveillance cameras can see the slasher.
/// </summary>
[ByRefEvent]
public sealed class SlasherIncorporealCameraCheckEvent : EntityEventArgs
{
    public SlasherIncorporealCameraCheckEvent(NetEntity slasher, float range)
    {
        Slasher = slasher;
        Range = range;
    }

    /// <summary>
    /// The slasher attempting to go incorporeal.
    /// </summary>
    public NetEntity Slasher { get; }

    /// <summary>
    /// Range to check for active cameras with line of sight.
    /// </summary>
    public float Range { get; }

    /// <summary>
    /// True if a camera can see the slasher.
    /// </summary>
    public bool Cancelled { get; set; }
}

