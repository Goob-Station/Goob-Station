using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._Mono.Species.Systems;

[Serializable, NetSerializable]
public sealed partial class CoolOffDoAfterEvent : SimpleDoAfterEvent;
