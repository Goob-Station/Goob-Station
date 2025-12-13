using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.CorticalBorer;

[Serializable, NetSerializable]
public sealed partial class CorticalInfestDoAfterEvent : SimpleDoAfterEvent;
