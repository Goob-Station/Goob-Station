namespace Content.Goobstation.Shared.Fishing.Components;

/// <summary>
/// The fish itself!
/// </summary>
[RegisterComponent]
public sealed partial class FishComponent : Component
{
    public const float DefaultDifficulty = 0.07f;
    public const float DefaultDifficultyVariety = 0.02f;

    [DataField]
    public float FishDifficulty = DefaultDifficulty;

    [DataField]
    public float FishDifficultyVarirty = DefaultDifficultyVariety;
}
