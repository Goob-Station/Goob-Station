using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Bloodsuckers.Components.Actions;
using Content.Goobstation.Shared.Bloodsuckers.Systems;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Movement.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Bloodsuckers.Systems;

public sealed class BloodsuckerFortitudeSystem : SharedBloodsuckerFortitudeSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedBloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityManager.EntityQueryEnumerator<BloodsuckerFortitudeComponent>();
        while (query.MoveNext(out var uid, out var fort))
        {
            if (!fort.Active)
                continue;

            if (_timing.CurTime < fort.UpdateTimer)
                continue;

            fort.UpdateTimer = _timing.CurTime + fort.UpdateDelay;
            Dirty(uid, fort);

            var ent = new Entity<BloodsuckerFortitudeComponent>(uid, fort);

            CheckFortitudeValidity(ent);
            if (!fort.Active)
                continue;

            DrainBlood(uid, fort);
            CheckRunning(ent);
        }
    }

    private void DrainBlood(EntityUid uid, BloodsuckerFortitudeComponent comp)
    {
        if (!TryComp(uid, out BloodstreamComponent? bloodstream))
            return;

        _bloodstream.TryModifyBloodLevel(
            new Entity<BloodstreamComponent?>(uid, bloodstream),
            -FixedPoint2.New(comp.BloodDrainPerSecond));
    }
}
