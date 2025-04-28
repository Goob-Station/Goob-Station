using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.CheckInfection;

[Serializable, NetSerializable]
public sealed partial class CheckInfectionDoAfter : SimpleDoAfterEvent;
