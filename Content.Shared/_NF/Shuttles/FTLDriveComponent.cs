using Content.Shared.Shuttles.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._NF.Shuttles;

/// <summary>
/// Assigned to shuttles that are able to FTL.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FTLDriveComponent : Component
{
    [DataField, AutoNetworkedField]
    public FTLDriveData Data = new (SharedShuttleSystem.FTLRange, false);
}

/// <summary>
/// Contains data for the FTL drive.
/// </summary>
[DataDefinition]
[Serializable, NetSerializable]
public partial record struct FTLDriveData
{
    public FTLDriveData(float range, bool ftlToSameMap)
    {
        Range = range;
        FTLToSameMap = ftlToSameMap;
    }

    [DataField]
    public float Range;

    [DataField("ftlToSameMap")]
    public bool FTLToSameMap;

    [DataField]
    public float? StartupTime;

    [DataField]
    public float? KnockdownTime;

    [DataField]
    public float? TravelTime;

    [DataField]
    public float? ArrivalTime;

    [DataField]
    public float? CooldownTime;
}
