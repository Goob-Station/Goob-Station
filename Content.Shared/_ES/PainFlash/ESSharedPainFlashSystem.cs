using Content.Shared._ES.PainFlash.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.FixedPoint;
using Robust.Shared.Timing;

namespace Content.Shared._ES.PainFlash;

public abstract class ESSharedPainFlashSystem : EntitySystem
{
    [Dependency] protected readonly IGameTiming Timing = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<ESPainFlashComponent, DamageChangedEvent>(OnDamageChanged);
    }

    protected abstract void OnDamageChanged(Entity<ESPainFlashComponent> ent, ref DamageChangedEvent args);

    public bool IsPainFlashTrigger(DamageChangedEvent args, out FixedPoint2 damage)
    {
        damage = FixedPoint2.Zero;

        if (!args.InterruptsDoAfters || args.DamageDelta is null)
            return false;

        var delta = DamageSpecifier.GetPositive(args.DamageDelta).GetTotal();
        damage = delta;

        return delta > 0;
    }
}
