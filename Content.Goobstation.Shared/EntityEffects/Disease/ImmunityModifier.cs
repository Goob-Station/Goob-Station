using Content.Goobstation.Shared.Disease.Chemistry;
using Content.Shared.EntityEffects;
using Content.Shared.EntityEffects.Effects;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.EntityEffects.Disease;

/// <summary>
/// Modifies the entity's immunity's strength, with accumulation.
/// </summary>
public sealed partial class ImmunityModifierSystem : EntityEffectSystem<ImmunityModifierMetabolismComponent, ImmunityModifier>
{
    [Dependency] private readonly IGameTiming _timing = default!;

    protected override void Effect(Entity<ImmunityModifierMetabolismComponent> entity, ref EntityEffectEvent<ImmunityModifier> args)
    {
        var comp = entity.Comp;

        comp.GainRateModifier = args.Effect.GainRateModifier;
        comp.StrengthModifier = args.Effect.StrengthModifier;

        var time = args.Effect.StatusLifetime * args.Scale;

        var offsetTime = Math.Max(comp.ModifierTimer.TotalSeconds, _timing.CurTime.TotalSeconds);
        comp.ModifierTimer = TimeSpan.FromSeconds(offsetTime + time);

        Dirty(entity);
    }
}

public sealed partial class ImmunityModifier : EntityEffectBase<ImmunityModifier>
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
    /// </summary>
    [DataField]
    public float StatusLifetime = 2f;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return Loc.GetString("reagent-effect-guidebook-immunity-modifier",
            ("chance", Probability),
            ("gainrate", GainRateModifier),
            ("strength", StrengthModifier),
            ("time", StatusLifetime));
    }
}
