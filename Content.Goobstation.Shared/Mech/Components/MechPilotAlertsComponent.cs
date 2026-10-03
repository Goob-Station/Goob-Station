using Content.Shared.Alert;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Mech.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class MechPilotAlertsComponent : Component
{
    [DataField]
    public ProtoId<AlertPrototype> PowerAlert = "MechPower";

    [DataField]
    public ProtoId<AlertPrototype> IntegrityAlert = "MechIntegrity";

    [DataField]
    public ProtoId<AlertPrototype> ChargingAlert = "MechCharging";

    [DataField]
    public float IntegrityWarnThreshold = 0.5f;

    [DataField]
    public float IntegrityDangerThreshold = 0.3f;

    [DataField]
    public float IntegrityCriticalThreshold = 0.15f;

    [DataField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(0.5);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan NextUpdate;

    [ViewVariables, AutoNetworkedField]
    public EntityUid? LastPilot;
}
