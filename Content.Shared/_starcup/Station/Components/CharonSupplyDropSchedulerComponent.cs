using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Shared._starcup.Station.Components;

/// <summary>
/// Schedules periodic CentCom supply drops for Charon-Epsilon stations.
/// </summary>
[RegisterComponent]
public sealed partial class CharonSupplyDropSchedulerComponent : Component
{
    /// <summary>
    /// Seconds between automatic supply drops.
    /// </summary>
    [DataField]
    public TimeSpan Interval = TimeSpan.FromMinutes(8);

    /// <summary>
    /// Game time at which the next drop should occur.
    /// </summary>
    [DataField]
    public TimeSpan? NextDrop;

    /// <summary>
    /// Possible drop-pod prototypes to spawn.
    /// </summary>
    [DataField]
    public List<EntProtoId> DropSpawners = new()
    {
        "DropPodCharonMedical",
        "DropPodCharonFood",
        "DropPodCharonEngineering",
        // "DropPodCharonScience",
        "DropPodCharonMaterials",
        "DropPodCharonSecurityCargo",
    };
}
