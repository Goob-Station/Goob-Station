using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Slasher.Events;
/// <summary>
/// A collection of various Slasher Related Events
/// </summary>

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

/// <summary>
/// DoAfter event raised when Soul Steal completes.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class SlasherSoulStealDoAfterEvent : SimpleDoAfterEvent;
