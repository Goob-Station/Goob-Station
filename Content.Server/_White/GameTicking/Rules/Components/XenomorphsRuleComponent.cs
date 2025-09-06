namespace Content.Server._White.GameTicking.Rules.Components;

[RegisterComponent]
public sealed partial class XenomorphsRuleComponent : Component
{
    [ViewVariables]
    public List<EntityUid> Xenomorphs = new ();

    #region Check

    [DataField]
    public TimeSpan CheckDelay = TimeSpan.FromSeconds(30);

    [ViewVariables]
    public TimeSpan NextCheck;

    #endregion

    #region Announcement

    [DataField]
    public string? Announcement = "xenomorphs-announcement";

    [DataField]
    public Color AnnouncementColor = Color.Red;

    [DataField]
    public string? NoMoreThreatAnnouncement = "xenomorphs-no-more-threat-announcement";

    [DataField]
    public Color NoMoreThreatAnnouncementColor = Color.Gold;

    [DataField]
    public string? Sender;

    [DataField]
    public TimeSpan MinTimeToAnnouncement = TimeSpan.FromSeconds(400);

    [DataField]
    public TimeSpan MaxTimeToAnnouncement = TimeSpan.FromSeconds(450);

    [ViewVariables]
    public bool Announced;

    [ViewVariables]
    public TimeSpan? AnnouncementTime;

    #endregion

    #region RoundEnd

    [DataField]
    public float XenomorphsShuttleCallPercentage = 0.7f;

    [DataField]
    public TimeSpan ShuttleCallTime = TimeSpan.FromMinutes(5);

    [DataField]
    public string RoundEndTextSender = "comms-console-announcement-title-centcom";

    [DataField]
    public string RoundEndTextShuttleCall = "xenomorphs-win-announcement-shuttle-call";

    [DataField]
    public string RoundEndTextAnnouncement = "xenomorphs-win-announcement";

    [DataField]
    public WinType WinType = WinType.Neutral;

    [DataField]
    public List<WinCondition> WinConditions = new ();

    #endregion
}

public enum WinType : byte
{
    XenoMajor,
    XenoMinor,
    Neutral,
    CrewMinor,
    CrewMajor
}

public enum WinCondition : byte
{
    NukeExplodedOnStation,
    NukeActiveInStation,
    XenoTakeoverStation,
    XenoInfiltratedOnCentCom,
    AllReproduceXenoDead,
    AllCrewDead
}
