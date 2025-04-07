// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 Vyacheslav Kovalevsky <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 keronshb <54602815+keronshb@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Server.Explosion.EntitySystems;
using Content.Server.Mind;
using Content.Server.Objectives.Components;
using Content.Server.Popups;
using Content.Server.Roles;
using Content.Shared.Ninja.Components;
using Content.Shared.Ninja.Systems;
using Content.Shared.Roles;
using Content.Shared.Sticky;

namespace Content.Server.Ninja.Systems;

/// <summary>
/// Prevents planting a spider charge outside of its location and handles greentext.
/// </summary>
public sealed class SpiderChargeSystem : SharedSpiderChargeSystem
{
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedRoleSystem _role = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SpaceNinjaSystem _ninja = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpiderChargeComponent, AttemptEntityStickEvent>(OnAttemptStick);
        SubscribeLocalEvent<SpiderChargeComponent, EntityStuckEvent>(OnStuck);
        SubscribeLocalEvent<SpiderChargeComponent, TriggerEvent>(OnExplode);
    }

    /// <summary>
    /// Require that the planter is a ninja and the charge is near the target warp point.
    /// </summary>
    private void OnAttemptStick(EntityUid uid, SpiderChargeComponent comp, ref AttemptEntityStickEvent args)
    {
        if (args.Cancelled)
            return;

        var user = args.User;

        if (!_mind.TryGetMind(args.User, out var mind, out _))
            return;

        if (!_role.MindHasRole<NinjaRoleComponent>(mind))
        {
            _popup.PopupEntity(Loc.GetString("spider-charge-not-ninja"), user, user);
            args.Cancelled = true;
            return;
        }

        // allow planting anywhere if there is no target, which should never happen
        if (!_mind.TryGetObjectiveComp<SpiderChargeConditionComponent>(user, out var obj) || obj.Target == null)
            return;

        // assumes warp point still exists
        var targetXform = Transform(obj.Target.Value);
        var locXform = Transform(args.Target);
        if (locXform.MapID != targetXform.MapID ||
            (_transform.GetWorldPosition(locXform) - _transform.GetWorldPosition(targetXform)).LengthSquared() > comp.Range * comp.Range)
        {
            _popup.PopupEntity(Loc.GetString("spider-charge-too-far"), user, user);
            args.Cancelled = true;
            return;
        }
    }

    /// <summary>
    /// Allows greentext to occur after exploding.
    /// </summary>
    private void OnStuck(EntityUid uid, SpiderChargeComponent comp, ref EntityStuckEvent args)
    {
        comp.Planter = args.User;
    }

    /// <summary>
    /// Handles greentext after exploding.
    /// Assumes it didn't move and the target was destroyed so be nice.
    /// </summary>
    private void OnExplode(EntityUid uid, SpiderChargeComponent comp, TriggerEvent args)
    {
        if (!TryComp<SpaceNinjaComponent>(comp.Planter, out var ninja))
            return;

        // assumes the target was destroyed, that the charge wasn't moved somehow
        _ninja.DetonatedSpiderCharge((comp.Planter.Value, ninja));
    }
}