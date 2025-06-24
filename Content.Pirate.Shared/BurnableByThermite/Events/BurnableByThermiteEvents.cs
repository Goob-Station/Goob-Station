using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Pirate.Shared.BurnableByThermite.Events;

[Serializable, NetSerializable]
public sealed partial class BurnableByThermiteBeakerDoAfterEvent : SimpleDoAfterEvent
{
}
