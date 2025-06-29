using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._Shitmed.Tourniquet;

[Serializable, NetSerializable]
public sealed partial class TourniquetDoAfterEvent : SimpleDoAfterEvent
{
}

[Serializable, NetSerializable]
public sealed partial class RemoveTourniquetDoAfterEvent : SimpleDoAfterEvent
{
}
