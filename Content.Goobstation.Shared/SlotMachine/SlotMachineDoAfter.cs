using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.SlotMachine;

[Serializable, NetSerializable]
public sealed partial class SlotMachineDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class SlotMachineEmagDoAfterEvent : SimpleDoAfterEvent;
