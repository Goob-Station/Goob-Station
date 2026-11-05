using Content.Goobstation.Shared.MartialArts.Components;
using Content.Goobstation.Shared.MartialArts.Specific.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.EntityEffects;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.MartialArts.Effects;

public sealed partial class SleepingCarpGnashEntityEffectSystem : EntityEffectSystem<DamageableComponent, SleepingCarpGnash>
{
    [Dependency] private DamageableSystem _damageable = default!;

    protected override void Effect(Entity<DamageableComponent> target, ref EntityEffectEvent<SleepingCarpGnash> args)
    {
        // Hacky to make this work
        if (!TryComp(target, out MartialArtsComboPerformedOnComponent? comp))
            throw new InvalidOperationException("No MartialArtsComboPerformedOnComponent on target!");

        var performer = comp.Performer;
        var performedOn = target;

        if (!TryComp(performer, out SleepingCarpGnasherComponent? gnasherComp))
            return;

        var damage = args.Effect.Damage;

        foreach (var (k, v) in damage.DamageDict)
        {
            damage.DamageDict[k] = v + gnasherComp.ConsecutiveGnashes * 5;
        }

        _damageable.ChangeDamage(performedOn.AsNullable(), damage);

        gnasherComp.ConsecutiveGnashes++;
        Dirty(performer, gnasherComp);
    }
}

[UsedImplicitly]
public sealed partial class SleepingCarpGnash : EntityEffectBase<SleepingCarpGnash>
{
    [DataField(required: true)]
    public DamageSpecifier Damage = default!;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => "Placeholder";
}
