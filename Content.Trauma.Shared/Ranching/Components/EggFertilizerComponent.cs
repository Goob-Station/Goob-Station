namespace Content.Trauma.Shared.Ranching.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class EggFertilizerComponent : Component
{
    [DataField]
    public float DoAfter = 15f;

    [DataField]
    public EntProtoId? SpecialReplacement;

    [DataField]
    public EntProtoId? SpecialReplacementRequiredEgg;
}
