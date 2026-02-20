// SPDX-FileCopyrightText: 2025 BombasterDS <deniskaporoshok@gmail.com>
// SPDX-FileCopyrightText: 2025 BombasterDS2 <shvalovdenis.workmail@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Marcus F <199992874+thebiggestbruh@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 the biggest bruh <199992874+thebiggestbruh@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Objectives.Components;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;
using Content.Server.GameTicking.Rules;
using Robust.Shared.Random;
using Content.Server._Goobstation.Objectives.Components;
using Content.Shared.Mind.Filters;


namespace Content.Server.Objectives.Systems;

/// <summary>
/// Handles assinging a target to an objective entity with <see cref="TargetObjectiveComponent"/> using different components.
/// These can be combined with condition components for objective completions in order to create a variety of objectives.
/// </summary>
public sealed class PickObjectiveTargetSystem : EntitySystem
{
    [Dependency] private readonly TargetObjectiveSystem _target = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly TraitorRuleSystem _traitorRule = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PickSpecificPersonComponent, ObjectiveAssignedEvent>(OnSpecificPersonAssigned);
        SubscribeLocalEvent<PickRandomPersonComponent, ObjectiveAssignedEvent>(OnRandomPersonAssigned);
        SubscribeLocalEvent<PickRandomTraitorComponent, ObjectiveAssignedEvent>(OnRandomTraitorAssigned);
        SubscribeLocalEvent<RandomTraitorTargetComponent, ObjectiveAssignedEvent>(OnRandomTraitorTargetAssigned);
    }

    private void OnSpecificPersonAssigned(Entity<PickSpecificPersonComponent> ent, ref ObjectiveAssignedEvent args)
    {
        // invalid objective prototype
        if (!TryComp<TargetObjectiveComponent>(ent.Owner, out var target))
        {
            args.Cancelled = true;
            return;
        }

        // target already assigned
        if (target.Target != null)
            return;

        if (args.Mind.OwnedEntity == null)
        {
            args.Cancelled = true;
            return;
        }

        var user = args.Mind.OwnedEntity.Value;
        if (!TryComp<TargetOverrideComponent>(user, out var targetComp) || targetComp.Target == null)
        {
            args.Cancelled = true;
            return;
        }

        _target.SetTarget(ent.Owner, targetComp.Target.Value);
    }

    private void OnRandomPersonAssigned(Entity<PickRandomPersonComponent> ent, ref ObjectiveAssignedEvent args)
    {
        // invalid objective prototype
        if (!TryComp<TargetObjectiveComponent>(ent, out var target))
        {
            args.Cancelled = true;
            return;
        }

        // target already assigned
        if (target.Target != null)
            return;

        // couldn't find a target :(
        if (_mind.PickFromPool(ent.Comp.Pool, ent.Comp.Filters, args.MindId) is not {} picked)
        {
            args.Cancelled = true;
            return;
        }

        if (_mind.PickFromPool(new AliveHumansPool(), new(), args.MindId) is not {} fallback)
        // fallback if no traitors
        _target.SetTarget(ent, picked, target);
    }

    private void OnRandomTraitorAssigned(Entity<PickRandomTraitorComponent> ent, ref ObjectiveAssignedEvent args)
    {
        // invalid objective prototype
        if (!TryComp<TargetObjectiveComponent>(ent, out var target))
        {
            args.Cancelled = true;
            return;
        }

        // target already assigned
        if (target.Target != null)
            return;

        if (!TryComp<MindComponent>(args.MindId, out var mind))
        {
            args.Cancelled = true;
            return;
        }

        var otherTraitors = _traitorRule.GetOtherTraitorMindsAliveAndConnected(mind);

        EntityUid? picked = null;
        if (otherTraitors.Count > 0)
        {
            picked = _random.Pick(otherTraitors).Id;
        }

        // fallback if no traitors this should never happen if there are enough players
        if (picked == null)
        {
            if (_mind.PickFromPool(new AliveHumansPool(), new(), args.MindId) is not {} fallback)
            {
                args.Cancelled = true;
                return;
            }
            picked = fallback;
        }

        _target.SetTarget(ent, picked.Value, target);
    }

    private void OnRandomTraitorTargetAssigned(Entity<RandomTraitorTargetComponent> ent, ref ObjectiveAssignedEvent args)
    {
        // invalid objective prototype
        if (!TryComp<TargetObjectiveComponent>(ent, out var target))
        {
            args.Cancelled = true;
            return;
        }

        // target already assigned
        if (target.Target != null)
            return;

        if (!TryComp<MindComponent>(args.MindId, out var mind))
        {
            args.Cancelled = true;
            return;
        }

        var otherTraitors = _traitorRule.GetOtherTraitorMindsAliveAndConnected(mind);

        var possibleTargets = new HashSet<EntityUid>();

        foreach (var (_, traitorMind) in otherTraitors)
        {
            foreach (var obj in traitorMind.Objectives)
            {
                if (!TryComp<TargetObjectiveComponent>(obj, out var objTarget) || objTarget.Target == null)
                    continue;

                if (!HasComp<KillPersonConditionComponent>(obj))
                    continue;

                possibleTargets.Add(objTarget.Target.Value);
            }
        }

        // couldn't find a target :(
        if (possibleTargets.Count == 0)
        {
            args.Cancelled = true;
            return;
        }

        var picked = _random.Pick(possibleTargets);
        _target.SetTarget(ent, picked, target);
    }
}
