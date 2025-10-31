using Robust.Shared.Audio;

namespace Content.Goobstation.Server.Anomaly;

/// <summary>
/// This is used for the bald anomaly
/// </summary>
[RegisterComponent]
public sealed partial class BaldAnomalyComponent : Component
{
    /// <summary>
    /// mange of anomaly
    /// </summary>
    [DataField]
    public float BaseRange = 10f;

    /// <summary>
    /// Sound emitted when someone is made bald
    /// </summary>
    [DataField]
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/Items/scissors.ogg");

    [DataField]
    public int SpeakChance = 50;
}
