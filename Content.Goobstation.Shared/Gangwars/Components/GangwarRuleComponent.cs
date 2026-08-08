using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Gangwars.Components;

/// <summary>
/// Handles gang wars events like tracking claimed colors, duffelbag dropoffs, alerts, end of round cleaup, and so on.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GangwarRuleComponent : Component
{
    #region Claimed gangs and scores
    /// <summary>
    /// Stores claimed gang names and colors
    /// Used to prevent duplicates along with displaying the names on the leaderboard.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<Color, string> GangNames = new();

    /// <summary>
    /// How often gang scores are recalculated.
    /// </summary>
    [DataField]
    public TimeSpan ScoreUpdateInterval = TimeSpan.FromMinutes(2);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField]
    public TimeSpan NextScoreUpdate = TimeSpan.Zero;

    /// <summary>
    /// Cached total scores per gang color (tile points + member gang points).
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<Color, int> GangScores = new();
    #endregion

    #region Gang Crates
    /// <summary>
    /// How long after the gamerule starts before the first gang crate drops.
    /// </summary>
    [DataField]
    public TimeSpan GangCrateDropDelay = TimeSpan.FromMinutes(8);

    /// <summary>
    /// How long between subsequent gang crate drops.
    /// </summary>
    [DataField]
    public TimeSpan GangCrateDropInterval = TimeSpan.FromMinutes(22);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField]
    public TimeSpan NextGangCrateDrop = TimeSpan.Zero;

    [DataField]
    public string GangCratePrototype = "CrateGang";
    #endregion

    #region Duffelbags
    /// <summary>
    /// How long between gang duffel bag drops.
    /// </summary>
    [DataField]
    public TimeSpan GangDuffelbagDropInterval = TimeSpan.FromMinutes(10);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField]
    public TimeSpan NextGangDuffelbagDrop = TimeSpan.Zero;

    [DataField]
    public string GangDuffelbagPrototype = "GangDuffelBag";

    /// <summary>
    /// How long after a "Tip Off" purchase before the early duffel bag drop lands.
    /// </summary>
    [DataField]
    public TimeSpan TipOffDropDelay = TimeSpan.FromSeconds(10);

    /// <summary>
    /// How long before the gang radio announcement the tipped-off civilian receives their NanoChat message.
    /// </summary>
    [DataField]
    public TimeSpan TipOffHeadstartDelay = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Pending tip-off location name for the delayed radio announcement.
    /// </summary>
    [DataField]
    public string? TipOffPendingLocation;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField]
    public TimeSpan TipOffAnnounceAt = TimeSpan.Zero;
    #endregion

    #region Spawning stuff

    /// <summary>
    /// Duffelbag drops will not spawn within this range (in tiles) of any gang locker.
    /// </summary>
    [DataField]
    public float LockerExclusionRange = 10f;

    /// <summary>
    /// The radio channel ID to announce duffelbag drops on.
    /// </summary>
    [DataField]
    public string GangRadioChannel = "Gang";

    /// <summary>
    /// The name shown as the sender of the duffelbag drop radio announcement.
    /// </summary>
    [DataField]
    public string AnnouncerName = "Boss";

    [DataField]
    public SoundSpecifier EventAnnouncementSound
        = new SoundPathSpecifier("/Audio/_Goobstation/Gangs/hip_hop_beat.ogg")
        {
            Params = AudioParams.Default.WithVolume(-8f)
        };
    #endregion
}
