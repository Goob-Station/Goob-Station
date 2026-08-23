using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Cinematic;

/// <summary>
/// A scripted cinematic.
/// </summary>
[Prototype]
public sealed partial class CinematicPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public List<CinematicSegment> Segments = new();

    /// <summary>
    /// While the focus player watches, every other sound source is lowered.
    /// </summary>
    [DataField]
    public bool DuckAudio = true;

    /// <summary>
    /// How many decibels everything else is pulled lowered.
    /// </summary>
    [DataField]
    public float DuckDecibels = 24f;

    /// <summary>
    /// Components applied to the focus entity for the whole timeline.
    /// Don't use this if you want to add the component for a specific segment.
    /// It would remove it after the segment is over.
    /// </summary>
    [DataField]
    public ComponentRegistry? AddComp;
}

[DataDefinition]
public sealed partial class CinematicSegment
{
    /// <summary>
    /// Seconds this segment runs for.
    /// </summary>
    [DataField(required: true)]
    public float Duration;

    /// <summary>
    /// Played (for the focus player only) as the segment begins.
    /// </summary>
    [DataField]
    public SoundSpecifier? Sound;

    /// <summary>
    /// Components added to the focus entity for this segments duration and removed with it.
    /// (Cinematic zoom, cinematic letterbox, etc).
    /// </summary>
    [DataField]
    public ComponentRegistry? AddComp;
}
