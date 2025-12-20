using Content.Goobstation.Shared.Disease.Chemistry;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Disease.Effects;

/// <summary>
/// Modifies the entity's immunity's strength, with accumulation.
/// </summary>
public sealed partial class ImmunityModifier : EntityEffect
{
    /// <summary>
    /// How much to add to the immunity's gain rate.
    /// </summary>
    [DataField]
    public float GainRateModifier = 0.002f;

    /// <summary>
    /// How much to add to the immunity's strength.
    /// </summary>
    [DataField]
    public float StrengthModifier = 0.02f;

    /// <summary>
    /// How long the modifier applies (in seconds).
    /// Is scaled by reagent amount if used with an EntityEffectReagentArgs.
    /// </summary>
    [DataField]
    public float StatusLifetime = 2f;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return Loc.GetString("reagent-effect-guidebook-immunity-modifier",
            ("chance", Probability),
            ("gainrate", GainRateModifier),
            ("strength", StrengthModifier),
            ("time", StatusLifetime));
    }

    /// <summary>
    /// Remove reagent at set rate, changes the immunity modifiers and adds a ImmunityModifierMetabolismComponent if not already there.
    /// </summary>
    public override void Effect(EntityEffectBaseArgs args)
    {
        var status = args.EntityManager.EnsureComponent<ImmunityModifierMetabolismComponent>(args.TargetEntity);

        status.GainRateModifier = GainRateModifier;
        status.StrengthModifier = StrengthModifier;

        // only going to scale application time
        var statusLifetime = StatusLifetime;

        if (args is EntityEffectReagentArgs reagentArgs)
        {
            statusLifetime *= reagentArgs.Scale.Float();
        }

        IncreaseTimer(status, statusLifetime, args.EntityManager, args.TargetEntity);
    }
    public void IncreaseTimer(ImmunityModifierMetabolismComponent status, float time,IEntityManager entityManager, EntityUid uid)
    {
        var gameTiming = IoCManager.Resolve<IGameTiming>();

        var offsetTime = Math.Max(status.ModifierTimer.TotalSeconds, gameTiming.CurTime.TotalSeconds);

        status.ModifierTimer = TimeSpan.FromSeconds(offsetTime + time);
        entityManager.Dirty(uid, status);
    }
}
