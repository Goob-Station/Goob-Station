namespace Content.Goobstation.Shared.Fishing.Components;

/// <summary>
/// The fish itself!
/// </summary>
[RegisterComponent]
public sealed partial class FishComponent : Component
{
    public const float DefaultDifficulty = -0.06f;
    public const float DefaultDifficultyVariety = 0.016f;

    [DataField]
    public float FishDifficulty = DefaultDifficulty;

    [DataField]
    public float FishDifficultyVarirty = DefaultDifficultyVariety;
}
