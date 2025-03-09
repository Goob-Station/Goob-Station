namespace Content.Shared._Goobstation.Fishing.Components;

/// <summary>
/// Applied to players that are pulling fish out from water
/// </summary>
[RegisterComponent]
public sealed partial class ActiveFisherComponent : Component
{
    [DataField]
    public float TotalProgress = 0.5f;

    [DataField]
    public float ProgressPerUse = 0.07f;

    [DataField]
    public float ProgressWithdraw = -0.2f;

    [DataField]
    public EntityUid FishingRod;
}
