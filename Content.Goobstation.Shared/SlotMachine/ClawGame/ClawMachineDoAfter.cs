using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.SlotMachine.ClawGame;

[Serializable, NetSerializable]
public sealed partial class ClawGameDoAfterEvent : SimpleDoAfterEvent;
