using Content.Goobstation.Shared.MartialArts.Components;
using Content.Goobstation.Shared.MartialArts.Specific;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.EntityEffects;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.MartialArts.Effects;

/// <summary>
/// Some hardcoding is required here i guess
/// </summary>
public sealed partial class MartialArtsDamageEntityEffectSystem : EntityEffectSystem<DamageableComponent, MartialArtsDamage>
{
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private MartialArtsSystem _martialArts = default!;
    [Dependency] private CapoeiraSystem _capoeira = default!;

    protected override void Effect(Entity<DamageableComponent> target, ref EntityEffectEvent<MartialArtsDamage> args)
    {
        // Hacky to make this work
        if (!TryComp(target, out MartialArtsComboPerformedOnComponent? comp))
            throw new InvalidOperationException("No MartialArtsComboPerformedOnComponent on target!");

        var performer = comp.Performer;
        var performedOn = target;

        if (!TryComp<TargetingComponent>(performer, out var targetingComponent))
            return;

        var damage = args.Effect.Damage;

        if (args.Effect.UsesVelocityPower)
        {
            _capoeira.CanPerformCapoeiraMove(
                performer.Owner,
                args.Effect.MinPower,
                args.Effect.MaxPower,
                args.Effect.MinVelocity,
                args.Effect.VelocityPowerMultiplier,
                out var power
            );

            damage *= power;
        }

        if (TryComp(performer, out MartialArtModifiersComponent? modifiers))
        {
            var (mult, mod) = _martialArts.GetMultiplierModifier((performer, modifiers), MartialArtModifierType.Damage, false);
            var modifierSet = _martialArts.GetDamageModifierSet(damage, mult, mod);
            damage = DamageSpecifier.ApplyModifierSet(damage, modifierSet);
        }

        _damageable.TryChangeDamage(performedOn.AsNullable(),
            damage,
            origin: performer,
            targetPart: args.Effect.TargetPart ?? targetingComponent.Target);
    }
}

[UsedImplicitly]
public sealed partial class MartialArtsDamage : EntityEffectBase<MartialArtsDamage>
{
    [DataField(required: true)]
    public DamageSpecifier Damage;

    [DataField]
    public TargetBodyPart? TargetPart;

    /// <summary>
    /// For Capoeira.......
    /// </summary>
    [DataField]
    public bool UsesVelocityPower = false;

    [DataField]
    public float MinVelocity = 0;

    [DataField]
    public float MinPower = 1f;

    [DataField]
    public float MaxPower = 4f;

    [DataField]
    public float VelocityPowerMultiplier = 0.6f;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => "Placeholder";
}
