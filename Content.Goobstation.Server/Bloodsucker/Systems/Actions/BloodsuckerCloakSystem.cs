using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Bloodsuckers.Components;
using Content.Goobstation.Shared.Bloodsuckers.Components.Actions;
using Content.Goobstation.Shared.Bloodsuckers.Systems;
using Content.Goobstation.Shared.LightDetection.Components;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Damage;
using Content.Shared.Movement.Components;
using Content.Shared.Popups;
using Robust.Server.Audio;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Bloodsuckers.Systems;

public sealed class BloodsuckerCloakSystem : SharedBloodsuckerCloakSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedBloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityManager.EntityQueryEnumerator<BloodsuckerCloakComponent, LightDetectionComponent>();
        while (query.MoveNext(out var uid, out var cloak, out var light))
        {
            if (_timing.CurTime < cloak.UpdateTimer)
                continue;

            cloak.UpdateTimer = _timing.CurTime + cloak.UpdateDelay;
            Dirty(uid, cloak);

            if (!cloak.Active)
                continue;

            var ent = new Entity<BloodsuckerCloakComponent>(uid, cloak);

            // Drop cloak on unconsciousness
            CheckCloakValidity(ent);
            if (!cloak.Active)
                continue;

            // Stepped into light drop the cloak
            if (light.OnLight)
            {
                _popup.PopupEntity(
                    Loc.GetString("bloodsucker-cloak-light-exposed"),
                    uid, uid, PopupType.MediumCaution);
                Deactivate(ent);
                continue;
            }

            // In darkness drain blood and check for running
            DrainBlood(uid, cloak);
            CheckRunning(ent);
        }
    }

    private void DrainBlood(EntityUid uid, BloodsuckerCloakComponent comp)
    {
        if (!TryComp(uid, out BloodstreamComponent? bloodstream))
            return;

        _bloodstream.TryModifyBloodLevel(
            new Entity<BloodstreamComponent?>(uid, bloodstream),
            -FixedPoint2.New(comp.BloodDrainPerSecond));
    }

    private void CheckRunning(Entity<BloodsuckerCloakComponent> ent)
    {
        if (!TryComp(ent.Owner, out InputMoverComponent? mover) || !mover.Sprinting)
            return;

        var damage = new DamageSpecifier();
        damage.DamageDict["Blunt"] = ent.Comp.RunPenaltyDamage;

        _damageable.TryChangeDamage(ent.Owner, damage, ignoreResistances: true);

        _popup.PopupEntity(
            Loc.GetString("bloodsucker-cloak-run-penalty"),
            ent.Owner, ent.Owner, PopupType.SmallCaution);
    }
}
