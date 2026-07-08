
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Common.Barks;

[Prototype]
public sealed partial class BarkPrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;

    /// <summary>
    /// A list of sound files that are used for barks.
    /// </summary>
    [DataField(required: true)]
    public string Name = string.Empty;

    /// <summary>
    /// A list of sound files that are used for barks.
    /// </summary>
    [DataField(required: true)]
    public SoundCollectionSpecifier? SoundCollection;

    /// <summary>
    /// A list of species that can use this bark.
    /// </summary>
    [DataField]
    public HashSet<String>? SpeciesWhitelist;

    /// <summary>
    /// The lower bound of the pitch variation.
    /// </summary>
    [DataField]
    public float MinPitch = 0.9f;

    /// <summary>
    /// The upper bound of the pitch variation.
    /// </summary>
    [DataField]
    public float MaxPitch = 1.1f;

    /// <summary>
    /// The volume of the bark.
    /// </summary>
    [DataField]
    public float Volume = 1;

    /// <summary>
    /// How often to play a sound.
    /// </summary>
    [DataField]
    public float Frequency = 0.5f;

    /// <summary>
    /// Stop the currently playing sound before playing a new one.
    /// </summary>
    [DataField]
    public bool Stop = false;

    /// <summary>
    /// Makes the audio predictable via hashing.
    /// </summary>
    [DataField]
    public bool Predictable = true;

    /// <summary>
    /// Whether it is available for selection in the character editor.
    /// </summary>
    [DataField]
    public bool RoundStart = true;
}
