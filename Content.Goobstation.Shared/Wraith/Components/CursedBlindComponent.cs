using Robust.Shared.GameStates;

[RegisterComponent, NetworkedComponent]
public sealed partial class CursedBlindComponent : Component
{
    /// <summary>
    /// How long before the blindness curse ticks and becomes stronger.
    /// </summary>
    [DataField]
    public TimeSpan TimeTillIncrement = TimeSpan.FromSeconds(5);

    /// <summary>
    /// How many stacks of blindness the curse has applied.
    /// </summary>
    [DataField]
    public int BlindnessStacks = 0;

    /// <summary>
    /// Maximum stacks before target is considered fully blinded.
    /// </summary>
    [DataField]
    public int MaxStacks = 7;

    /// <summary>
    /// Needed for death curse later. Gets set to true once max stacks are in.
    /// </summary>
    [DataField]
    public bool BlindCurseFullBloom;

    /// <summary>
    /// Next time at which blindness should increment.
    /// </summary>
    public TimeSpan NextTick = TimeSpan.Zero;
}
