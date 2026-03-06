using Content.Goobstation.Shared.Cult;
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
    [DataField] public BloodCultTier CurrentTier;

    [DataField] public BloodCultTier? ScheduledTier;

    /// <summary>
    ///     Percentage of crew converted needed to trigger a tier change.
    /// </summary>
    [DataField]
    public Dictionary<float, BloodCultTier> TierPercentageRatio = new()
    {
        { 0, BloodCultTier.None },
        { 0.2f, BloodCultTier.Eyes }, // 20%
        { 0.4f, BloodCultTier.Halos } // 40%
    };

    /// <summary>
    ///     Used in debugging with no mind check since you can't just start 10 clients at once.
    /// </summary>
    [DataField]
    public Dictionary<int, BloodCultTier> DebugCultistsTierRatio = new()
    {
        { 0, BloodCultTier.None },
        { 4, BloodCultTier.Eyes },
        { 9, BloodCultTier.Halos },
    };

    [ViewVariables(VVAccess.ReadOnly)] public int ReviveRuneCharges = 0;

    public Dictionary<BloodCultTier, CultTierData> TierData = new()
    {
        {
            BloodCultTier.Eyes,
            new("cult-tier-eyes", new SoundPathSpecifier("/Audio/_Goobstation/Ambience/Antag/bloodcult_eyes.ogg"))
        },
        {
            BloodCultTier.Halos,
            new("cult-tier-halos", new SoundPathSpecifier("/Audio/_Goobstation/Ambience/Antag/bloodcult_halos.ogg"))
        },
    };
}
