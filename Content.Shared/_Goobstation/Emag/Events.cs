using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Emag;

[Serializable, NetSerializable]
public sealed partial class EmergencyShuttleConsoleEmagDoAfterEvent : SimpleDoAfterEvent;
