namespace Content.Shared._Goobstation.Fishing.Components;

[RegisterComponent]
public sealed partial class FishingLureComponent : Component
{
    [DataField]
    public EntityUid FishingRod;

    [DataField]
    public EntityUid? FishingSpot;
}
