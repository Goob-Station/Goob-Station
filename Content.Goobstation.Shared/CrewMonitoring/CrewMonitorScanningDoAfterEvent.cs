using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.CrewMonitoring;

[Serializable, NetSerializable]
public sealed partial class CrewMonitorScanningDoAfterEvent : SimpleDoAfterEvent
{
}
