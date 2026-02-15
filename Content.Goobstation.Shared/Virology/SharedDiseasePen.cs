using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Virology;

[Serializable, NetSerializable]
public enum DiseasePenVisuals : byte
{
    Used,
}

[Serializable, NetSerializable]
public sealed partial class DiseasePenInjectEvent : SimpleDoAfterEvent;
