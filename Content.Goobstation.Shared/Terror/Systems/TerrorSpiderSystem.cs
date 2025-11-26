using Content.Goobstation.Shared.Terror.Components;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Robust.Shared.Timing;
using Robust.Shared.GameObjects;

namespace Content.Goobstation.Shared.Terror.Systems;

public sealed class TerrorSpiderSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damage = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<TerrorSpiderComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            comp.RegenAccumulator += frameTime;

            if (comp.RegenAccumulator < comp.RegenCooldown.TotalSeconds)
                continue;

            comp.RegenAccumulator = 0f;

            // Ensure the entity has a DamageableComponent
            if (!TryComp(uid, out DamageableComponent? damageable))
                continue;

            var amount = comp.TerrorRegen * (1 + comp.WrappedAmount);
            _damage.TryChangeDamage(uid, amount, damageable: damageable, targetPart: TargetBodyPart.All);


        }
    }
}

