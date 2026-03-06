using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Terror.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class PurpleTerrorComponent : Component
{
    /// <summary>
    /// Time accumulator for interval ticking.
    /// </summary>
    public float DamageAccumulator = 0f;

    /// <summary>
    /// Counts how many damage ticks have happened while out of range.
    /// Spider is gibbed at 15.
    /// </summary>
    public int DeathCounter = 0;

    /// <summary>
    /// Max allowed distance from a queen before damage starts ticking.
    /// </summary>
    [DataField]
    public float MaxDistance = 20f;

    /// <summary>
    /// Time between each damage tick.
    /// </summary>
    [DataField]
    public TimeSpan DamageInterval = TimeSpan.FromSeconds(2);
}
