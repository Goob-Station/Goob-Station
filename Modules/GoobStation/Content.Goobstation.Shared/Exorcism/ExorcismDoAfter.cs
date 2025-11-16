using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Exorcism;

[Serializable, NetSerializable]
public sealed partial class ExorcismDoAfterEvent : SimpleDoAfterEvent;
