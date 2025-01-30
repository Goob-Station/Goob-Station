using Content.Server.Administration.Logs;
using Content.Server.Body.Systems;
using Content.Server.Explosion.EntitySystems;
using Content.Shared.Body.Part;
using Content.Shared.Clothing.Components;
using Content.Shared.Damage;
using Content.Shared.Database;
using Content.Shared.Electrocution;
using Content.Shared.Explosion.Components;
using Content.Shared.Inventory;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using System;

namespace Content.Server.Electrocution;

public sealed class ExplosiveShockSystem : EntitySystem
{
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly ExplosionSystem _explosion = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ExplosiveShockComponent, InventoryRelayedEvent<ElectrocutionAttemptEvent>>(OnElectrocuted);
    }

    private void OnElectrocuted(EntityUid uid, ExplosiveShockComponent explosiveShock, InventoryRelayedEvent<ElectrocutionAttemptEvent> args)
    {
        if (!TryComp<ExplosiveComponent>(uid, out var explosive))
            return;

        _popup.PopupEntity(Loc.GetString("explosive-shock-sizzle", ("item", uid)), uid);
        _adminLogger.Add(LogType.Electrocution, $"{ToPrettyString(args.Args.TargetUid):entity} triggered explosive shock item {ToPrettyString(uid):entity}");
        Timer.Spawn(TimeSpan.FromSeconds(explosiveShock.SizzleTime), () => TimerEnd(uid, explosiveShock));
    }

    private void TimerEnd(EntityUid uid, ExplosiveShockComponent explosiveShock) {
        if (Deleted(uid) || !TryComp<ExplosiveComponent>(uid, out var explosive))
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

            _stun.TryKnockdown(target.Value, TimeSpan.FromSeconds(explosiveShock.KnockdownTime), true);
        }
    }
}
