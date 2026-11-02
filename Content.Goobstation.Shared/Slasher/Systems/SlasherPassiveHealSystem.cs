using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Slasher.Components;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Slasher.Systems;

public sealed partial class SlasherPassiveHealSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private SlasherFearSystem _fear = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<SlasherPassiveHealComponent, DamageableComponent>();
        while (query.MoveNext(out var uid, out var heal, out var damageable))
        {
            if (heal.NextHeal > curTime)
                continue;

            heal.NextHeal = curTime + heal.Interval;
            Dirty(uid, heal);

            if (damageable.TotalDamage <= FixedPoint2.Zero
                || _mobState.IsDead(uid)
                || TryComp<SlasherIncorporealComponent>(uid, out var incorporeal) && incorporeal.IsIncorporeal)
                continue;

            var healPerSecond = MathHelper.Lerp(heal.MinHealPerSecond, heal.MaxHealPerSecond, _fear.GetFearFraction(uid));
            var amount = FixedPoint2.New(healPerSecond * (float) heal.Interval.TotalSeconds);
            var spec = BuildHeal(damageable, amount);
            if (spec.Empty)
                continue;

            _damageable.ChangeDamage((uid, damageable), spec, true, false, targetPart: TargetBodyPart.All, splitDamage: heal.SplitBehavior);
        }
    }

    private static DamageSpecifier BuildHeal(DamageableComponent damageable, FixedPoint2 amount)
    {
        var spec = new DamageSpecifier();
        var total = damageable.TotalDamage;

        foreach (var (type, value) in damageable.Damage.DamageDict)
        {
            if (value <= FixedPoint2.Zero)
                continue;

            var share = amount * (value / total);
            if (share > FixedPoint2.Zero)
                spec.DamageDict[type] = -share;
        }

        return spec;
    }
}
