using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.Medical.Components;

public sealed partial class HealthAnalyzerComponent
{
    /// <summary>
    /// Whether the health analyzer has a speaker. For body scanner.
    /// </summary>
    [DataField]
    public bool HasSpeaker = false;

    /// <summary>
    /// Localization message for the health analyzer speaker.
    /// </summary>
    [DataField]
    public string SpeakerMessage = "health-analyzer-speaker-message";

    /// <summary>
    /// When the next speaker message will be.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan SpeakerNextMessage = TimeSpan.Zero;

    /// <summary>
    /// How often the speaker speaks.
    /// </summary>
    [DataField]
    public TimeSpan SpeakerUpdateRate = TimeSpan.FromSeconds(5);
}