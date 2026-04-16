using Robust.Shared.Audio;

namespace Content.Goobstation.Shared.Terror.Gamerules;

[RegisterComponent]
public sealed partial class TerrorHiveRuleComponent : Component
{
    /// <summary>
    /// Basically who is the Queen of this particular Hive.
    /// </summary>
    [DataField]
    public EntityUid? Queen;

    /// <summary>
    /// The amount of corpses wrapped up by the Hive.
    /// </summary>
    [DataField]
    public int TotalWrapped;

    /// <summary>
    /// How many corpses are required to be wrapped for the terrors to be outed.
    /// </summary>
    [DataField]
    public int RequiredWrapsForAnnouncement = 10;

    /// <summary>
    /// How many corpses are required to be wrapped for the terrors to "win".
    /// </summary>
    [DataField]
    public int RequiredWrapsForWin = 50;

    /// <summary>
    /// If the spiders already got snitched on by the announcer.
    /// </summary>
    [DataField]
    public bool InfestationAnnounced;

    /// <summary>
    /// If the spiders have won.
    /// </summary>
    [DataField]
    public bool RoundWon;

    /// <summary>
    /// If the hive has lost.
    /// </summary>
    [DataField]
    public bool HiveDefeated;

    [DataField]
    public SoundSpecifier? DetectedAudio = new SoundPathSpecifier("/Audio/_Goobstation/Announcements/outbreak_terror.ogg");

    [DataField]
    public SoundSpecifier? CriticalAudio = new SoundPathSpecifier("/Audio/StationEvents/blobin_time.ogg");
}
