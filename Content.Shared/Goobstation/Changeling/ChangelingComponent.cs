using Content.Shared.Humanoid;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.Changeling;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class ChangelingComponent : Component
{
    [DataField("soundRegenerate")]
    public List<SoundSpecifier?> SoundPool = new()
    {
        new SoundPathSpecifier("/Audio/Effects/gib1.ogg"),
        new SoundPathSpecifier("/Audio/Effects/gib2.ogg"),
        new SoundPathSpecifier("/Audio/Effects/gib3.ogg"),
    };

    /// <summary>
    ///     Current amount of chemicals changeling currently has.
    /// </summary>
    [DataField("chemicals"), AutoNetworkedField]
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

    public float ChemicalRegenerationMobStateModifier = 0;
    /// <summary>
    ///     Modifier for chemical regeneration. Positive = faster, negative = slower.
    /// </summary>
    public float ChemicalRegenerationModifier = 0;


    public List<TransformData> AbsorbedDNA = new();

    /// <summary>
    ///     Maximum amount of DNA a changeling can absorb.
    /// </summary>
    public int MaxAbsorbedDNA = 5;
}

public struct TransformData
{
    public string Name;

    public string Fingerprint;

    public string DNA;

    public HumanoidAppearanceComponent HumanoidAppearanceComp;
}
