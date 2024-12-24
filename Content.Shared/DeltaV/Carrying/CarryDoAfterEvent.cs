using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.DeltaV.Carrying;

[Serializable, NetSerializable]
public sealed partial class CarryDoAfterEvent : SimpleDoAfterEvent;
