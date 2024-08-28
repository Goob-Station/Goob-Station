using Content.Server.RoundEnd;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.GameTicking.Rules.Components;

/// <summary>
/// Component for the RevolutionaryRuleSystem that stores info about winning/losing, player counts required for starting, as well as prototypes for Revolutionaries and their gear.
/// </summary>
[RegisterComponent, Access(typeof(RevolutionaryRuleSystem))]
public sealed partial class RevolutionaryRuleComponent : Component
{
    /// <summary>
    /// When the round will if all the command are dead (Incase they are in space)
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan CommandCheck;

    /// <summary>
    /// The amount of time between each check for command check.
    /// </summary>
    [DataField]
    public TimeSpan TimerWait = TimeSpan.FromSeconds(20);

    /// <summary>
    /// The time it takes after the last head is killed for the shuttle to arrive.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan ShuttleCallTime = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Needed to stop checking and announce spam in the end of round
    /// </summary>
    [DataField]
    public bool HasAnnouncementPlayed = false;

    /// <summary>
    /// Text for shuttle call if RoundEndBehavior is ShuttleCall.
    /// </summary>
    [DataField]
    public string RevolutionariesLoseEndTextShuttleCall = "revolutionaries-lose-announcement-shuttle-call"; //GoobStation

    /// <summary>
    /// Text for announcement if RoundEndBehavior is ShuttleCall. Used if shuttle is already called
    /// </summary>
    [DataField]
    public string RevolutionariesLoseRoundEndTextAnnouncement = "revolutionaries-lose-announcement"; //GoobStation

    /// <summary>
    /// Text for shuttle call if revs win.
    /// </summary>
    [DataField]
    public string RevolutionariesWinEndTextShuttleCall = "revolutionaries-win-announcement-shuttle-call"; //GoobStation

    /// <summary>
    /// Text for shuttle call if revs win. Used if shuttle is already called.
    /// </summary>
    [DataField]
    public string RevolutionariesWinEndText = "revolutionaries-win-announcement"; //GoobStation

    /// <summary>
    /// Text for round end sender if revs win.
    /// </summary>
    [DataField]
    public string RevolutionariesWinSender = "revolutionaries-win-sender"; //GoobStation
}
