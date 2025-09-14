// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 unknown <Administrator@DESKTOP-PMRIVVA.kommune.indresogn.no>
//
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

namespace Content.Goobstation.Server.Bingle;

public sealed class BingleSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
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
        var cords = Transform(uid).Coordinates;
        if (!cords.IsValid(EntityManager) || cords.Position == Vector2.Zero)
            return;
        if (MapId.Nullspace == Transform(uid).MapID)
            return;

        if (component.Prime)
            component.MyPit = Spawn("BinglePit", cords);
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
        //Prevent Friendly Bingle fire
        if (HasComp<BinglePitComponent>(args.Target) || HasComp<BingleComponent>(args.Target))
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
