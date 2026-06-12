using Content.Goobstation.Common.Temperature.Components;
using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Server.Disease;
using Content.Goobstation.Shared.Bloodsuckers.Components;
using Content.Goobstation.Shared.Bloodsuckers.Components.Actions;
using Content.Goobstation.Shared.Bloodsuckers.Systems;
using Content.Goobstation.Shared.SpecialPassives.BoostedImmunity;
using Content.Goobstation.Shared.Wraith.Curses;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Popups;
using Content.Shared.StatusEffectNew;
using Content.Shared.Temperature;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Bloodsuckers.Systems;

public sealed class BloodsuckerMasqueradeSystem : SharedBloodsuckerMasqueradeSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedBloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly DiseaseSystem _disease = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityManager.EntityQueryEnumerator<BloodsuckerMasqueradeComponent>();
        while (query.MoveNext(out var uid, out var masq))
        {
            if (!masq.Active)
                continue;

            if (_timing.CurTime < masq.UpdateTimer)
                continue;

            masq.UpdateTimer = _timing.CurTime + masq.UpdateDelay;
            Dirty(uid, masq);

            var ent = new Entity<BloodsuckerMasqueradeComponent>(uid, masq);
            CheckMasqueradeValidity(ent);
            if (!masq.Active)
                continue;

            DrainBlood(uid, masq);
        }
    }

    protected override void Activate(Entity<BloodsuckerMasqueradeComponent> ent)
    {
        base.Activate(ent);
    }

    protected override void Deactivate(Entity<BloodsuckerMasqueradeComponent> ent)
    {
        base.Deactivate(ent);

        _disease.TryCureAll(ent.Owner);
    }

    protected override void RemoveBloodsuckerPassives(EntityUid uid)
    {
        RemComp<CurseImmuneComponent>(uid);
        RemComp<SpecialLowTempImmunityComponent>(uid);
    }

    protected override void RestoreBloodsuckerPassives(EntityUid uid)
    {
        EnsureComp<CurseImmuneComponent>(uid);
        EnsureComp<SpecialLowTempImmunityComponent>(uid);
    }

    private void DrainBlood(EntityUid uid, BloodsuckerMasqueradeComponent comp)
    {
        if (!TryComp(uid, out BloodstreamComponent? bloodstream))
            return;

        _bloodstream.TryModifyBloodLevel(
            new Entity<BloodstreamComponent?>(uid, bloodstream),
            -FixedPoint2.New(comp.BloodDrainPerSecond));
    }
}
