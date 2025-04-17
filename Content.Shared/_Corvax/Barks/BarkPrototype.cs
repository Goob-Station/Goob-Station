using Robust.Shared.Prototypes;

namespace Content.Shared._Corvax.Speech.Synthesis;

/// <summary>
/// A prototype for the available barges.
/// </summary>
[Prototype("bark")]
public sealed class BarkPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = default!;

    /// <summary>
    /// The name of the voice.
    /// </summary>
    [DataField("name")]
    public string Name { get; } = string.Empty;

    /// <summary>
    /// A set of sounds used for speech.
    /// </summary>
    [DataField("soundFiles", required: true)]
    public List<string> SoundFiles { get; } = new();

    /// <summary>
    /// Whether it is available for selection.
    /// </summary>
    [DataField("roundStart")]
    public bool RoundStart { get; } = true;
}
