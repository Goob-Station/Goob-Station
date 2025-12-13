using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Harvestable;


[Serializable, NetSerializable]
public sealed partial class HarvestedDoAfterEvent : SimpleDoAfterEvent;
