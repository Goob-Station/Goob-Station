namespace Content.Shared._Goobstation.Fishing.Components;

/// <summary>
/// The fish itself!
/// </summary>
[RegisterComponent]
public sealed partial class FishComponent : Component
{
    public const float DefaultDifficulty = -0.1f;
    public const float DefaultDifficultyVariety = 0.02f;

    [DataField]
    public float FishDifficulty = DefaultDifficulty;

    [DataField]
    public float FishDifficultyVarirty = DefaultDifficultyVariety;
}
