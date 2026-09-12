namespace Content.Shared._Shitmed.StatusEffects;

[RegisterComponent]
public sealed partial class RandomBodySizeComponent : Component
{
    [DataField]
    public float MinHeight = 0.75f;

    [DataField]
    public float MaxHeight = 1.5f;

    [DataField]
    public float MinWidth = 0.75f;

    [DataField]
    public float MaxWidth = 1.5f;
}
