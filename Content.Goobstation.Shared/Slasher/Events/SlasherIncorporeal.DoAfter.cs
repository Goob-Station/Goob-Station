using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Slasher.Events;

[Serializable, NetSerializable]
public sealed partial class SlasherIncorporealizeDoAfterEvent : SimpleDoAfterEvent
{
}
