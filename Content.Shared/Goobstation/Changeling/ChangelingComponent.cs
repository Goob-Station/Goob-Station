using Robust.Shared.GameStates;

namespace Content.Shared.Changeling;

[RegisterComponent, NetworkedComponent]
public sealed partial class ChangelingComponent : Component
{
    /// <summary>
    ///     Current amount of chemicals changeling currently has.
    /// </summary>
    [DataField("chemicals")]
    public float Chemicals = 100;

    /// <summary>
    ///     Maximum amount of chemicals changeling can have.
    /// </summary>
    [DataField("maxChemicals")]
    public float MaxChemicals = 100;

    public float ChemicalRegenerationAccumulator = 0;

    /// <summary>
    ///     Time in seconds to take before chemical regeneration occurs.
    /// </summary>
    public readonly float ChemicalRegenerationTimer = 1;

    /// <summary>
    ///     Modifier for chemical regeneration. Positive = faster, negative = slower.
    /// </summary>
    public float ChemicalRegenerationModifier = 0;


    public List<EntityUid> AbsorbedDNA = new();

    /// <summary>
    ///     Maximum amount of DNA a changeling can absorb.
    /// </summary>
    public int MaxAbsorbedDNA = 5;
}
