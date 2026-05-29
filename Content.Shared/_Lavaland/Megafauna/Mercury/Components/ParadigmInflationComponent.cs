using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna.Mercury.Components;

/// <summary>
/// Basically picks a target, checks their damage values, and if it isn't genetic, then heal that damage, and make it genetic.
/// Can be avoided by not having any damage, or by having genetic already as your highest damage type.
/// </summary>

[RegisterComponent]
public sealed partial class ParadigmInflationComponent : Component
{
    /// <summary>
    /// How long the target gets analyzed for.
    /// </summary>
    [DataField]
    public float AnalyzeTime = 5f;

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
