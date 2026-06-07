using Content.Shared._White.Xenomorphs.Acid;
using Content.Shared._White.Xenomorphs.Acid.Components;
using Content.Shared.Damage;

namespace Content.Server._White.Xenomorphs.Acid;

public sealed class XenomorphAcidSystem : SharedXenomorphAcidSystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;

    public override void Update(float frameTime)
    {
        var time = Timing.CurTime;

        var acidCorrodingQuery = EntityQueryEnumerator<AcidCorrodingComponent>();
        while (acidCorrodingQuery.MoveNext(out var uid, out var acidCorrodingComponent))
        {
            if (time > acidCorrodingComponent.NextDamageAt)
            {
                _damageable.TryChangeDamage(uid, acidCorrodingComponent.DamagePerSecond);
                acidCorrodingComponent.NextDamageAt = time + TimeSpan.FromSeconds(1);
            }

            if (time <= acidCorrodingComponent.AcidExpiresAt)
                continue;

            QueueDel(acidCorrodingComponent.Acid);
            RemCompDeferred<AcidCorrodingComponent>(uid);
        }
    }
}
