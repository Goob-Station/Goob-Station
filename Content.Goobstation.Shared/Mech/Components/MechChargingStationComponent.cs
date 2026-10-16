using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Mech.Components;

/// <summary>
/// Charges any mechs that are in the same tile as the charging station.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class MechChargingStationComponent : Component
{
    [DataField]
    public float APCMechChargeRate = 105f;

    [DataField]
    public float APCIdleDraw = 1f;

    [DataField]
    public float ChargeRangeLimit = 0.75f;

    [DataField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(1);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan NextUpdate;

    [ViewVariables, AutoNetworkedField]
    public bool IsCharging;

    [ViewVariables]
    public HashSet<EntityUid> ParkedMechs = new();
}

[Serializable, NetSerializable]
public enum MechChargingStationVisuals : byte
{
    Charging,
}
