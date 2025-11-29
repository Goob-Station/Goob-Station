using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server._CorvaxGoob.Announcer;

[Prototype("announcer")]
public sealed class AnnouncerPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private init; } = default!;

    /// <summary>
    /// Will generate unique calendar for announcer that will apply for entire day if it's coincidence.
    /// </summary>
    [DataField]
    public bool RandomCalendarEnabled { get; private set; } = true;

    /// <summary>
    /// Minimum count of announcer days in month.
    /// </summary>
    [DataField]
    public int MinDaysInMonth { get; private set; } = 3;

    /// <summary>
    /// Maximum count of announcer days in month.
    /// </summary>
    [DataField]
    public int MaxDaysInMonth { get; private set; } = 7;

    /// <summary>
    /// Chance to be selected in a certain day.
    /// </summary>
    [DataField]
    public float Chance { get; private set; } = 0.3f;

    /// <summary>
    /// Will play sound on shuttle recall.
    /// </summary>
    [DataField]
    public SoundSpecifier? ShuttleRecallSound;

    /// <summary>
    /// Will play sound on shuttle call.
    /// </summary>
    [DataField]
    public SoundSpecifier? ShuttleCallSound;

    /// <summary>
    /// Will play sound on shuttle docked.
    /// </summary>
    [DataField]
    public SoundSpecifier? ShuttleDockedSound;

    /// <summary>
    /// Will play sound on centcom announcement.
    /// </summary>
    [DataField]
    public SoundSpecifier? CentcomAnnouncementSound;
}
