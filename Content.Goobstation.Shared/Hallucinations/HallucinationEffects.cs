using Content.Shared.Dataset;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Hallucinations;

/// <summary>
/// Spawns phantom mobs near the victim.
/// </summary>
public sealed partial class MobHallucinationEffect : HallucinationEffect
{
    [DataField(required: true)]
    public List<EntProtoId> Mobs = new();

    [DataField]
    public int CountMin = 1;

    [DataField]
    public int CountMax = 2;

    /// <summary>
    /// How far from the victim a phantom spawns.
    /// </summary>
    [DataField]
    public float RangeMin = 3f;

    [DataField]
    public float RangeMax = 6f;

    /// <summary>
    /// How close a phantom must be to swing at the victim.
    /// </summary>
    [DataField]
    public float AttackRange = 1.5f;

    [DataField]
    public float AttackDelayMin = 1.5f;

    [DataField]
    public float AttackDelayMax = 3f;

    [DataField]
    public SoundSpecifier? AttackSound = new SoundCollectionSpecifier("Punch");
}

/// <summary>
/// Plays a sound from a random point near the victim.
/// </summary>
public sealed partial class SoundHallucinationEffect : HallucinationEffect
{
    [DataField(required: true)]
    public SoundSpecifier Sound = default!;

    [DataField]
    public float RangeMin = 2f;

    [DataField]
    public float RangeMax = 7f;
}

/// <summary>
/// Fake conversations.
/// </summary>
public sealed partial class SpeechHallucinationEffect : HallucinationEffect
{
    /// <summary>
    /// Dataset of lines a hallucinated voice can say.
    /// </summary>
    [DataField]
    public ProtoId<LocalizedDatasetPrototype> Messages = "HallucinationSpeech";

    /// <summary>
    /// If true, everyone nearby chants the same line several times each instead of one person saying it once.
    /// </summary>
    [DataField]
    public bool Mass;

    [DataField]
    public float Range = 6f;

    /// <summary>
    /// In mass mode, how many times each nearby voice repeats the line.
    /// </summary>
    [DataField]
    public int RepeatMin = 3;

    [DataField]
    public int RepeatMax = 9;
}

/// <summary>
/// Adds a fake held item / worn clothing sprite layer to a random nearby bystander for a few seconds.
/// </summary>
public sealed partial class AppearanceHallucinationEffect : HallucinationEffect
{
    [DataField(required: true)]
    public List<string> Prototypes = new();

    [DataField(required: true)]
    public List<string> States = new();

    [DataField]
    public SoundSpecifier? Sound;
}

/// <summary>
/// Network message for AppearanceHallucinationEffect.
/// </summary>
[Serializable, NetSerializable]
public sealed class SetHallucinationAppearanceMessage(List<string> prototypes, List<string> states, SoundSpecifier? sound)
    : EntityEventArgs
{
    public List<string> Prototypes = prototypes;
    public List<string> States = states;
    public SoundSpecifier? Sound = sound;
}

/// <summary>
/// Fully replaces a random nearby bystander's sprite with one of these mobs for a few seconds.
/// </summary>
public sealed partial class MorphHallucinationEffect : HallucinationEffect
{
    [DataField(required: true)]
    public List<EntProtoId> Mobs = new();
}

/// <summary>
/// Network message for MorphHallucinationEffect.
/// </summary>
[Serializable, NetSerializable]
public sealed class SetHallucinationMorphMessage(List<string> mobs) : EntityEventArgs
{
    public List<string> Mobs = mobs;
}
