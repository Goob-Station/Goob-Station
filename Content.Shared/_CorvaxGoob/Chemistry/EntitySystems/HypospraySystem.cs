using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.Chemistry.Hypospray.Events;

[Serializable, NetSerializable]
public sealed partial class HyposprayTryInjectDoAfterEvent : SimpleDoAfterEvent
{

}
