using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Spy;

[Serializable, NetSerializable]
public sealed partial class SpyStealDoAfterEvent : SimpleDoAfterEvent;
