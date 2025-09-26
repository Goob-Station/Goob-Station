using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

[RegisterComponent, NetworkedComponent]
public sealed partial class CursedBlindComponent : Component
{
    /// <summary>
    /// How long before the blindness curse ticks and becomes stronger.
    /// </summary>
    [DataField]
    public TimeSpan TimeTillIncrement = TimeSpan.FromSeconds(10);

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
    /// Checks if they are fully blind before applying further stacks.
    /// </summary>
    [DataField]
    public bool FullyBlind;

    /// <summary>
    /// Next time at which blindness should increment.
    /// </summary>
    public TimeSpan NextTick = TimeSpan.Zero;

    /*
    /// <summary>
    /// The status icon prototype displayed for revolutionaries
    /// </summary>
    [DataField]
    public ProtoId<DiseaseIconPrototype> StatusIcon = "BlindCurseIcon";*/
}
