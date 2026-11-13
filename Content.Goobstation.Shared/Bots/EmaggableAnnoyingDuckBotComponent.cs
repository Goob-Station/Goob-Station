using Content.Shared.Dataset;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Bots;

/// <summary>
/// Replaces the annoying duck bot's sound, quack/egg timings and honk sound when emagged.
/// </summary>
[RegisterComponent]
public sealed partial class EmaggableAnnoyingDuckBotComponent : Component
{
    [DataField]
    public SoundSpecifier? Sound;

    [DataField(required: true)]
    public TimeSpan MinSoundInterval;

    [DataField(required: true)]
    public TimeSpan MaxSoundInterval;

    [DataField(required: true)]
    public float EggLayCooldownMin;

    [DataField(required: true)]
    public float EggLayCooldownMax;

    [DataField(required: true)]
    public TimeSpan MinVocalizeInterval;

    [DataField(required: true)]
    public TimeSpan MaxVocalizeInterval;

    [DataField]
    public ProtoId<LocalizedDatasetPrototype>? Dataset;
}
