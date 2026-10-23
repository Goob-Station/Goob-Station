using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.LimbGnawing;

[Serializable, NetSerializable]
public sealed partial class LimbGnawDoAfterEvent : SimpleDoAfterEvent;
