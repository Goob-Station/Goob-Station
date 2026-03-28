using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Trauma.Shared.Ranching.Events;

[Serializable, NetSerializable]
public sealed partial class FertilizeDoAfterEvent : SimpleDoAfterEvent;
