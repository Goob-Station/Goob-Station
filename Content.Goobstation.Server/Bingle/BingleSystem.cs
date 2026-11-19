// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Goobstation.Common.Bingle;
using Content.Goobstation.Shared.Bingle;
using Content.Server.Polymorph.Components;
using Content.Server.Polymorph.Systems;
using Content.Shared.CombatMode;
using Content.Shared.Flash.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.Actions;
using Content.Shared.Polymorph;
using Content.Shared.Actions.Events;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Content.Goobstation.Shared.Bongle;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Bingle;

public sealed partial class BingleSystem : EntitySystem
{
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private PolymorphSystem _polymorph = default!;
    [Dependency] private AppearanceSystem _appearance = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private MobStateSystem _mob = default!;

    private readonly HashSet<Entity<BongleComponent>> _bongles = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BingleComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<BingleComponent, AttackAttemptEvent>(OnAttackAttempt);
        SubscribeLocalEvent<BingleComponent, Shared.Overlays.ToggleNightVisionEvent>(OnNightvision);
        SubscribeLocalEvent<BingleComponent, ToggleCombatActionEvent>(OnCombatToggle);
        SubscribeLocalEvent<BingleComponent, BingleUpgradeActionEvent>(OnUpgradeAction);
    }

    private void OnMapInit(EntityUid uid, BingleComponent component, MapInitEvent args)
    {
        if (!component.LinkToPit)
            return;

        var cords = Transform(uid).Coordinates;
        if (!cords.IsValid(EntityManager) || cords.Position == Vector2.Zero)
            return;
        if (MapId.Nullspace == Transform(uid).MapID)
            return;

        if (component.Prime)
            component.MyPit = SpawnAtPosition("BinglePit", cords);
        else
        {
            var query = EntityQueryEnumerator<BinglePitComponent>();
            EntityUid? closestPit = null;
            float closestPitDistance = float.MaxValue; // This is literally done so the algorithm below doesn't spaz out
            while (query.MoveNext(out var queryUid, out var _))
            {
                Transform(queryUid).Coordinates.TryDistance(EntityManager, cords, out var closenessOfPit);
                if (closenessOfPit < closestPitDistance)
                {
                    closestPit = queryUid;
                    closestPitDistance = closenessOfPit;
                }
            }
            component.MyPit = closestPit;
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<BingleComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.NextRageCheck)
                continue;

            comp.NextRageCheck = _timing.CurTime + comp.RageCheckInterval;

            if (HasComp<BongleComponent>(uid) || !_mob.IsAlive(uid))
                continue;

            _bongles.Clear();
            _lookup.GetEntitiesInRange(Transform(uid).Coordinates, comp.RageRange, _bongles);

            var enraged = false;

            foreach (var (bongleUid, _) in _bongles)
            {
                if (!_mob.IsAlive(bongleUid) || !TryComp<BingleComponent>(bongleUid, out var bongleBingle))
                    continue;

                Enrage(bongleUid, bongleBingle);
                enraged = true;
            }

            if (enraged)
            {
                Enrage(uid, comp);
            }
        }
    }

    private void Enrage(EntityUid uid, BingleComponent comp)
    {
        if (!HasComp<SeethingComponent>(uid))
        {
            EntityManager.AddComponents(uid, comp.CompToAdd);
        }

        if (!TryComp<SeethingComponent>(uid, out var seething))
            return;

        seething.ExpireTime = _timing.CurTime + seething.Duration;
    }

    //ran by the pit to upgrade bingle damage
    public void UpgradeBingle(EntityUid uid, BingleComponent component)
    {
        if (component.Upgraded)
            return;

        _actions.AddAction(uid, "ActionBingleUpgrade", uid);

        _popup.PopupEntity(Loc.GetString("bingle-upgrade-success"), uid, uid);
        component.Upgraded = true;
    }

    private void OnUpgradeAction(EntityUid uid, BingleComponent component, BingleUpgradeActionEvent args)
    {
        // This is to support 1 unified way for every bingle variant to polymorph
        // Excuse the hard coding, I really wanted to just make a copy of BinglePolymorph prototype and use it
        // But holy fucking shit polymorph system sucks
        _polymorph.PolymorphEntity(uid, new PolymorphConfiguration
        {
            Entity = component.UpgradedID,
            Forced = true,
            TransferName = true,
            TransferHumanoidAppearance = false,
            Inventory = PolymorphInventoryChange.Drop,
            RevertOnDeath = false,
            RevertOnCrit = false,
        });
    }

    private void OnAttackAttempt(EntityUid uid, BingleComponent component, AttackAttemptEvent args)
    {
        //Prevent Friendly Bingle fire, and also allows bongles and bingles to fight
        // that's a lotta checks
        if ((HasComp<BinglePitComponent>(args.Target) && !HasComp<BongleComponent>(uid)) || (HasComp<BingleComponent>(args.Target) && HasComp<BongleComponent>(args.Target) == HasComp<BongleComponent>(uid)))
            args.Cancel();
    }

    private void OnNightvision(EntityUid uid, BingleComponent component, Shared.Overlays.ToggleNightVisionEvent args)
    {
        if (!TryComp<FlashImmunityComponent>(uid, out var flashComp))
            return;

        flashComp.Enabled = !flashComp.Enabled;
    }

    private void OnCombatToggle(EntityUid uid, BingleComponent component, ToggleCombatActionEvent args)
    {
        if (!TryComp<CombatModeComponent>(uid, out var combat))
            return;
        _appearance.SetData(uid, BingleVisual.Combat, combat.IsInCombatMode);
    }
}
