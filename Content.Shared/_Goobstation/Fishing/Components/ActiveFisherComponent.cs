using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Fishing.Components;

/// <summary>
/// Applied to players that are pulling fish out from water
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ActiveFisherComponent : Component
{
    [DataField]
    public TimeSpan? StartTime;

    [DataField]
    public TimeSpan? EndTime;

    [DataField]
    public TimeSpan? NextStruggle;

    [DataField]
    public float TotalProgress = 0f;

    [DataField, AutoNetworkedField]
    public float ProgressPerUse = 0.7f;

    [DataField, AutoNetworkedField]
    public EntityUid FishingRod;
}
