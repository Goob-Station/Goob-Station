namespace Content.Shared._Goobstation.Fishing.Components;

/// <summary>
/// The fish itself!
/// </summary>
[RegisterComponent]
public sealed partial class FishComponent : Component
{
    /// <summary>
    /// How much progress percentage this fish removes every tick from Active Fisher
    /// </summary>
    [DataField]
    public float FishDifficulty = 0.01f;
}
