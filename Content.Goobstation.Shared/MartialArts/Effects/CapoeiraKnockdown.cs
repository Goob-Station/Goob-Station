using Content.Goobstation.Shared.MartialArts.Components;
using Content.Goobstation.Shared.MartialArts.Specific;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.EntityEffects;
using Content.Shared.Stunnable;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.MartialArts.Effects;

public sealed partial class CapoeiraModifyKnockdownEntityEffectSystem : EntityEffectSystem<DamageableComponent, CapoeiraKnockdown>
{
    [Dependency] private CapoeiraSystem _capoeira = default!;
    [Dependency] private SharedStunSystem _stun = default!;

    protected override void Effect(Entity<DamageableComponent> target, ref EntityEffectEvent<CapoeiraKnockdown> args)
    {
        // Hacky to make this work
        if (!TryComp(target, out MartialArtsComboPerformedOnComponent? comp))
            throw new InvalidOperationException("No MartialArtsComboPerformedOnComponent on target!");

        var performer = comp.Performer;
        var performedOn = target;

        _capoeira.CanPerformCapoeiraMove(
            performer.Owner,
            args.Effect.MinPower,
            args.Effect.MaxPower,
            args.Effect.MinVelocity,
            args.Effect.VelocityPowerMultiplier,
            out var power
        );

        var time = args.Effect.Time * power;
        _stun.TryKnockdown(performedOn.Owner, time, drop: true);
        _stun.SetKnockdownTime(performedOn.Owner, time);
    }
}

[UsedImplicitly]
public sealed partial class CapoeiraKnockdown : EntityEffectBase<CapoeiraKnockdown>
{
    [DataField(required: true)]
    public TimeSpan Time;

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
