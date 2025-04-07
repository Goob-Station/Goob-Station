using Content.Shared.Administration.Logs;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Clothing.Components;
using Content.Shared.Damage;
using Content.Shared.Database;
using Content.Shared.Electrocution;
using Content.Shared.Explosion.Components;
using Content.Shared.Explosion.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Electrocution;

public sealed class ExplosiveShockSystem : EntitySystem
{
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedExplosionSystem _explosion = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ExplosiveShockComponent, InventoryRelayedEvent<ElectrocutionAttemptEvent>>(OnElectrocuted);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ExplosiveShockIgnitedComponent>();
        var now = _timing.CurTime;
        while (query.MoveNext(out var uid, out var ignited))
        {
            if (now >= ignited.ExplodeAt)
                TryExplode(uid);
        }
    }

    private void OnElectrocuted(EntityUid uid, ExplosiveShockComponent explosiveShock, InventoryRelayedEvent<ElectrocutionAttemptEvent> args)
    {
        if (!TryComp<ExplosiveComponent>(uid, out var explosive))
            return;

        _popup.PopupEntity(Loc.GetString("explosive-shock-sizzle", ("item", uid)), uid);
        _adminLogger.Add(LogType.Electrocution, $"{ToPrettyString(args.Args.TargetUid):entity} triggered explosive shock item {ToPrettyString(uid):entity}");
        EnsureComp<ExplosiveShockIgnitedComponent>(uid, out var ignited);
        ignited.ExplodeAt = _timing.CurTime + explosiveShock.ExplosionDelay;
    }

    private void TryExplode(EntityUid uid) {
        if (Deleted(uid) || !TryComp<ExplosiveComponent>(uid, out var explosive) || !TryComp<ExplosiveShockComponent>(uid, out var explosiveShock))
            return;

        EntityUid? target = null;
        if (TryComp<ClothingComponent>(uid, out var clothing) && clothing.InSlot != null)
            target = Transform(uid).ParentUid;

        _explosion.TriggerExplosive(uid, explosive);

        if (target != null)
        {
            // gloves go under armor so ignore resistances
            foreach (var part in _body.GetBodyChildrenOfType(target.Value, BodyPartType.Hand))
                _damageable.TryChangeDamage(part.Id, explosiveShock.HandsDamage, true);

            foreach (var part in _body.GetBodyChildrenOfType(target.Value, BodyPartType.Arm))
                _damageable.TryChangeDamage(part.Id, explosiveShock.ArmsDamage, true);

            _stun.TryKnockdown(target.Value, explosiveShock.KnockdownTime, true);
        }
    }
}
