using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Fishing.Components;

/// <summary>
/// Applied to players that are pulling fish out from water
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ActiveFisherComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan? StartTime;

    [DataField, AutoNetworkedField]
    public TimeSpan? EndTime;

    [DataField, AutoNetworkedField]
    public TimeSpan? NextStruggle;

    [DataField, AutoNetworkedField]
    public float TotalProgress;

    [DataField, AutoNetworkedField]
    public float ProgressPerUse = 0.7f;

    [DataField, AutoNetworkedField]
    public EntityUid FishingRod;
}
