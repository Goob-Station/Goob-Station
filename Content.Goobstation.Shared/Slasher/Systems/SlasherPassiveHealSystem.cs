using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Slasher.Components;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
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
    [Dependency] private SharedBloodstreamSystem _blood = default!;

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

            if (_mobState.IsDead(uid)
                || TryComp<SlasherIncorporealComponent>(uid, out var incorporeal) && incorporeal.IsIncorporeal)
                continue;

            var fear = _fear.GetFearFraction(uid);
            var seconds = (float) heal.Interval.TotalSeconds;

            if (damageable.TotalDamage > FixedPoint2.Zero)
            {
                var amount = FixedPoint2.New(MathHelper.Lerp(heal.MinHealPerSecond, heal.MaxHealPerSecond, fear) * seconds);
                var spec = BuildHeal(damageable, amount);
                if (!spec.Empty)
                    _damageable.ChangeDamage((uid, damageable), spec, true, false, targetPart: TargetBodyPart.All, splitDamage: heal.SplitBehavior);
            }

            if (!TryComp<BloodstreamComponent>(uid, out var bloodstream))
                continue;

            if (bloodstream.BleedAmount > 0f)
            {
                var reduction = MathHelper.Lerp(heal.MinBleedReductionPerSecond, heal.MaxBleedReductionPerSecond, fear) * seconds;
                _blood.TryModifyBleedAmount((uid, bloodstream), -MathF.Min(reduction, bloodstream.BleedAmount));
            }

            if (_blood.GetBloodLevel((uid, bloodstream)) < 1f)
            {
                var blood = MathHelper.Lerp(heal.MinBloodRestoredPerSecond, heal.MaxBloodRestoredPerSecond, fear) * seconds;
                _blood.TryModifyBloodLevel((uid, bloodstream), FixedPoint2.New(blood));
            }
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
