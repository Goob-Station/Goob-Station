// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Bingle;
using Content.Goobstation.Shared.Bingle;
using Content.Goobstation.Shared.Bongle;
using Content.Server.Polymorph.Systems;
using Content.Shared.Actions;
using Content.Shared.Actions.Events;
using Content.Shared.CombatMode;
using Content.Shared.Flash.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Polymorph;
using Content.Shared.Popups;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Timing;
using System.Numerics;

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

    private readonly HashSet<Entity<BingleComponent>> _bingles = new();

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

    [SubscribeLocalEvent]
    private void OnBongleStartup(Entity<BongleComponent> ent, ref ComponentStartup args)
    {
        EnsureComp<ActiveSeethingCheckComponent>(ent.Owner);
    }

    [SubscribeLocalEvent]
    private void OnBongleMobStateChanged(Entity<BongleComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Alive)
        {
            EnsureComp<ActiveSeethingCheckComponent>(ent.Owner);
        }
        else
        {
            RemCompDeferred<ActiveSeethingCheckComponent>(ent.Owner);
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ActiveSeethingCheckComponent, BingleComponent>();
        while (query.MoveNext(out var uid, out var active, out var comp))
        {
            if (_timing.CurTime < active.NextCheck)
                continue;

            active.NextCheck = _timing.CurTime + comp.RageCheckInterval;

            _bingles.Clear();
            _lookup.GetEntitiesInRange(Transform(uid).Coordinates, comp.RageRange, _bingles);

            var enraged = false;

            foreach (var (bingleUid, bingle) in _bingles)
            {
                if (HasComp<BongleComponent>(bingleUid) || !_mob.IsAlive(bingleUid))
                    continue;

                Enrage(bingleUid, bingle);
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
        if ((HasComp<BinglePitComponent>(args.Target) && !HasComp<BongleComponent>(uid)) // Only bongles can attack bingle pits
            || (HasComp<BingleComponent>(args.Target) && // newline or else comment too big
            HasComp<BongleComponent>(args.Target) == HasComp<BongleComponent>(uid))) // prevent friendly fire between bingles, and friendly fire between bongles
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
