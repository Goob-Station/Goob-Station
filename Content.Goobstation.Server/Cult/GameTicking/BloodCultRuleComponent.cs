using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Server.Cult.GameTicking;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class BloodCultRuleComponent : Component
{
    [DataField] public HashSet<EntityUid> Cultists = new();

    [DataField] public EntityUid? CultLeader;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan LeaderReselectionTimer = TimeSpan.FromMinutes(2);

    /// <summary>
    ///     How much time is required for tier changes to take effect after the announcement.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan TierChangeTimer = TimeSpan.FromMinutes(2);

    /// <summary>
    ///     Current tier of the cult.
    /// </summary>
    [DataField] public CultTier CurrentTier;

    [DataField]
    public Dictionary<float, CultTier> TierPercentageRatio = new()
    {
        { 0, CultTier.None },
        { 0.2f, CultTier.Eyes },
        { 0.4f, CultTier.Halos }
    };

    [DataField]
    public Dictionary<int, CultTier> DebugCultistsTierRatio = new()
    {
        { 0, CultTier.None },
        { 2, CultTier.Eyes },
        { 4, CultTier.Halos },
    };

    [ViewVariables(VVAccess.ReadOnly)] public int ReviveRuneCharges = 0;


    public enum CultTier
    {
        /// <summary>
        ///     No visual changes
        /// </summary>
        None,

        /// <summary>
        ///     Grr kitten... *eyes glow red* *and valid*
        /// </summary>
        Eyes,

        /// <summary>
        ///     VERY VALID halos start appearing
        /// </summary>
        Halos
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
