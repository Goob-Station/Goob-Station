using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Slasher.Events;

/// <summary>
/// DoAfter event raised when Soul Steal completes.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class SlasherSoulStealDoAfterEvent : SimpleDoAfterEvent
{
}
