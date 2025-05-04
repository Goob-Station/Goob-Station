using Content.Shared.Whitelist;

namespace Content.Goobstation.Shared.CrewMonitoring;

[RegisterComponent]
public sealed partial class CrewMonitorScanningComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public List<EntityUid> ScannedEntities = [];

    [DataField]
    public TimeSpan DoAfterTime = TimeSpan.FromSeconds(15);

    [DataField]
    public bool ApplyDeathrattle = true;

    [DataField]
    public EntityWhitelist Whitelist = new ();
}
