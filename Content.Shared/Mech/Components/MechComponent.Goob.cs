using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

namespace Content.Shared.Mech.Components;

public sealed partial class MechComponent
{
    [DataField]
    public EntityWhitelist? PilotBlacklist;

    [DataField]
    public EntityWhitelist? BatteryWhitelist;

    [ViewVariables, AutoNetworkedField]
    public bool OnChargingStation;

    [DataField]
    public EntProtoId SelectEquipmentAction = "ActionMechSelectEquipment";

    [ViewVariables]
    public Dictionary<EntityUid, EntityUid> EquipmentActions = new();
}
