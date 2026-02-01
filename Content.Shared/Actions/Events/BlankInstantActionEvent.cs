using Robust.Shared.Serialization;

namespace Content.Shared.Actions.Events;

/// <summary>
///     Goobstation - this exists to not screw up tests and whatnot.
///     TODO: Make instant actions support no events at all.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class BlankInstantActionEvent : InstantActionEvent;
