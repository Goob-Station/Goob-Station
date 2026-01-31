using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Terror.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class TerrorQueenComponent : Component
{
    /// <summary>
    /// Keeps track of how many creatures the hive as a whole has wrapped. Used to multiply the chance of a tier 2 or above to spawn.
    /// </summary>
    [DataField]
    public int HiveTotalWrappedAmount;

    /// <summary>
    /// Baseline starting chance for T2 eggs.
    /// </summary>
    [DataField]
    public float Tier2BaseChance = 0.05f;

    /// <summary>
    /// Maximum chance T2 eggs can asymptotically approach.
    /// </summary>
    [DataField]
    public float Tier2MaxChance = 0.50f;

    /// <summary>
    /// Baseline starting chance for T3 eggs.
    /// </summary>
    [DataField]
    public float Tier3BaseChance = 0.01f;

    /// <summary>
    /// Maximum chance T3 eggs can reach.
    /// </summary>
    [DataField]
    public float Tier3MaxChance = 0.10f;

    /// <summary>
    /// Curve constant controlling how fast Tier 2 chance approaches Tier2MaxChance.
    /// Higher means slower approach.
    /// </summary>
    [DataField]
    public float Tier2CurveK = 40f;

    /// <summary>
    /// Curve constant controlling how fast Tier 3 chance approaches Tier3MaxChance.
    /// </summary>
    [DataField]
    public float Tier3CurveK = 80f;

    [DataField]
    public SoundSpecifier DeathSound = new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_queen_died.ogg");
}
