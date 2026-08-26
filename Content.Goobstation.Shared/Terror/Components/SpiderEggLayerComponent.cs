namespace Content.Goobstation.Shared.Terror.Components;

/// <summary>
/// Accumulates eggs over time. Unlike the regular egg layer system, ignores hunger, because
/// this system is for terror spiders, but it's not like it can't be generic.
/// Also I didn't want to touch upstream.
/// </summary>
[RegisterComponent]
public sealed partial class SpiderEggLayerComponent : Component
{
    /// <summary>
    /// How many eggs are currently stored up.
    /// </summary>
    [DataField]
    public int StoredEggs;

    /// <summary>
    /// The maximum amount of stored eggs.
    /// </summary>
    [DataField]
    public int MaxStoredEggs = 5;

    /// <summary>
    /// Interval at which eggs are generated.
    /// </summary>
    [DataField(required: true)]
    public TimeSpan GenerationInterval = TimeSpan.FromSeconds(60);
    public TimeSpan NextGenerationTime;
}
