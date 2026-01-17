using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Server.Cult.GameTicking;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class BloodCultRuleComponent : Component
{
    [DataField] public List<EntityUid> Cultists = new();

    [DataField] public EntityUid? CultLeader;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan LeaderElectionTimer = TimeSpan.FromSeconds(30);

    /// <summary>
    ///    Time remaining until the next leader reselection occurs.
    ///    Is null if no election is currently scheduled.
    /// </summary>
    public TimeSpan? LeaderElectionCountdown = null;

    /// <summary>
    ///     How much time is required for tier changes to take effect after the announcement.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan TierChangeTimer = TimeSpan.FromMinutes(1);

    /// <summary>
    ///     Same as <see cref="LeaderElectionCountdown"/>, but for tier changes.
    /// </summary>
    public TimeSpan? TierChangeCountdown = null;

    /// <summary>
    ///     Current tier of the cult.
    /// </summary>
    [DataField] public CultTier CurrentTier;

    [DataField] public CultTier? ScheduledTier;

    [DataField]
    public Dictionary<float, CultTier> TierPercentageRatio = new()
    {
        { 0, CultTier.None },
        { 0.2f, CultTier.Eyes },
        { 0.4f, CultTier.Halos }
    };

    /// <summary>
    ///     Used in debugging with no mind check since you can't just start 10 clients at once.
    /// </summary>
    [DataField]
    public Dictionary<int, CultTier> DebugCultistsTierRatio = new()
    {
        { 0, CultTier.None },
        { 4, CultTier.Eyes },
        { 9, CultTier.Halos },
    };

    [ViewVariables(VVAccess.ReadOnly)] public int ReviveRuneCharges = 0;

    public Dictionary<CultTier, CultTierData> TierData = new()
    {
        {
            CultTier.Eyes,
            new("cult-tier-eyes", new SoundPathSpecifier("/Audio/_Goobstation/Ambience/Antag/bloodcult_eyes.ogg"))
        },
        {
            CultTier.Halos,
            new("cult-tier-halos", new SoundPathSpecifier("/Audio/_Goobstation/Ambience/Antag/bloodcult_halos.ogg"))
        },
    };

    public struct CultTierData
    {
        public LocId Announcement;
        public SoundSpecifier Sound;

        public CultTierData(string announcement, SoundSpecifier gainSound)
        {
            Announcement = announcement;
            Sound = gainSound;
        }
    }

    public enum CultTier
    {
        /// <summary>
        ///     No visual changes
        /// </summary>
        None = 0,

        /// <summary>
        ///     Grr kitten... *eyes glow red* *and valid*
        /// </summary>
        Eyes = 1,

        /// <summary>
        ///     VERY VALID halos start appearing
        /// </summary>
        Halos = 2
    }

    public enum WinType : byte
    {
        /// <summary>
        ///     NarSie got summoned.
        /// </summary>
        CultMajor,

        /// <summary>
        ///    Round ended, but all cult winning conditions were met.
        /// </summary>
        CultMinor,

        /// <summary>
        ///     Neutral. Cult didn't reach halos and/or escaped with the crew.
        /// </summary>
        Neutral,

        /// <summary>
        ///     All cultists were marooned and halos did not appear.
        /// </summary>
        CrewMinor,

        /// <summary>
        ///     All cultists were killed/deconverted.
        /// </summary>
        CrewMajor
    }
}
