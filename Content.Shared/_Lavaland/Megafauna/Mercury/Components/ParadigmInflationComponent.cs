using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna.Mercury.Components;

/// <summary>
/// Analyzes a target for a duration, then finds their highest damage type.
/// If the highest damage type is not genetic, removes that damage and applies the same amount as genetic damage.
/// Does nothing if the target has no damage or their highest damage type is genetic.
/// </summary>

[RegisterComponent]
public sealed partial class ParadigmInflationComponent : Component
{
    /// <summary>
    /// How long the target gets analyzed for.
    /// </summary>
    [DataField]
    public float AnalyzeTime = 5f;

    /// <summary>
    /// How much to divide the damage before applying it. Higher values mean less damage is dealt after calculations.
    /// </summary>
    [DataField]
    public float DivideDamage = 2f;

    public EntityUid? Target;

    /// <summary>
    /// The prototype of the warning, mostly for sprite reasons.
    /// </summary>
    [DataField]
    public EntProtoId WarningPrototype = "ParadigmInflationTarget";
    public EntityUid? WarningEntity;

    /// <summary>
    /// Sound played as it starts analyzing.
    /// </summary>
    [DataField]
    public SoundSpecifier AnalyzeSound = new SoundPathSpecifier("/Audio/_Lavaland/Mobs/Bosses/Mercury/communicating.ogg");

    /// <summary>
    /// Sound played when damage is dealt.
    /// </summary>
    [DataField]
    public SoundSpecifier ParadigmSound = new SoundPathSpecifier("/Audio/_Lavaland/Mobs/Bosses/Mercury/glitch.ogg");

    public float Accumulator;
    public bool IsAnalyzing;
}
