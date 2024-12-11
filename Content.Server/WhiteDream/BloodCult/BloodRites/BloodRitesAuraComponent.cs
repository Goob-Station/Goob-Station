using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server.WhiteDream.BloodCult.BloodRites;

[RegisterComponent]
public sealed partial class BloodRitesAuraComponent : Component
{
    /// <summary>
    ///     Total blood stored in the Aura.
    /// </summary>
    [DataField]
    public FixedPoint2 StoredBlood;

    /// <summary>
    ///     Ratio which is applied to calculate the <see cref="StoredBlood"/> amount to regenerate blood in someone.
    /// </summary>
    [DataField]
    public float BloodRegenerationRatio = 0.1f;

    /// <summary>
    ///     Ratio which is applied to calculate the <see cref="StoredBlood"/> amount to heal yourself.
    /// </summary>
    [DataField]
    public float SelfHealRatio = 2f;

    /// <summary>
    ///     The amount of blood that is extracted from a person on using it on them.
    /// </summary>
    [DataField]
    public FixedPoint2 BloodExtractionAmount = 30f;

    /// <summary>
    ///     Time required to extract blood of something with bloodstream.
    /// </summary>
    [DataField]
    public TimeSpan BloodExtractionTime = TimeSpan.FromSeconds(5);

    /// <summary>
    ///     How much <see cref="StoredBlood"/> is consumed on healing.
    /// </summary>
    [DataField]
    public FixedPoint2 HealingCost = 40;

    /// <summary>
    ///     How much damage each use of the hand will heal. Will heal literally anything. Nar'sien magic, you know.
    /// </summary>
    [DataField]
    public FixedPoint2 TotalHealing = 20;

    [DataField]
    public float PuddleConsumeRadius = 0.5f;

    [DataField]
    public SoundSpecifier BloodRitesAudio = new SoundPathSpecifier(
        new ResPath("/Audio/WhiteDream/BloodCult/rites.ogg"),
        AudioParams.Default.WithVolume(-3));

    [DataField]
    public Dictionary<EntProtoId, float> Crafts = new()
    {
        ["BloodSpear"] = 300
    };

    public DoAfterId? ExtractDoAfterId;
}
