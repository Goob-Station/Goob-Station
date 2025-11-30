using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.GunSpinning;

[Serializable, NetSerializable]
public sealed partial class GunSpinDoAfterEvent : SimpleDoAfterEvent;