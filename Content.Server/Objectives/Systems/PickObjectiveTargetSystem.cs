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
using Content.Server.Revolutionary.Components;
using Robust.Shared.Random;
using System.Linq;
using Content.Shared.NukeOps;

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
        SubscribeLocalEvent<PickRandomHeadComponent, ObjectiveAssignedEvent>(OnRandomHeadAssigned);

        SubscribeLocalEvent<RandomTraitorProgressComponent, ObjectiveAssignedEvent>(OnRandomTraitorProgressAssigned);
        SubscribeLocalEvent<RandomTraitorAliveComponent, ObjectiveAssignedEvent>(OnRandomTraitorAliveAssigned);
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
        if (!TryComp<TargetObjectiveComponent>(ent.Owner, out var target))
        {
            args.Cancelled = true;
            return;
        }

        // target already assigned
        if (target.Target != null)
            return;

        var allHumans = _mind.GetAliveHumans(args.MindId,
            ent.Comp.NeedsOrganic, ent.Comp.ExcludeChangeling); // Goob edit - exclude IPCs and/or changelings

        // Can't have multiple objectives to kill the same person
        foreach (var objective in args.Mind.Objectives)
        {
            if (HasComp<KillPersonConditionComponent>(objective) && TryComp<TargetObjectiveComponent>(objective, out var kill))
            {
                allHumans.RemoveWhere(x => x.Owner == kill.Target);
            }
        }

        // no other humans to kill
        if (allHumans.Count == 0)
        {
            args.Cancelled = true;
            return;
        }

        _target.SetTarget(ent.Owner, _random.Pick(allHumans), target);
    }

    private void OnRandomHeadAssigned(Entity<PickRandomHeadComponent> ent, ref ObjectiveAssignedEvent args)
    {
        // invalid prototype
        if (!TryComp<TargetObjectiveComponent>(ent.Owner, out var target))
        {
            args.Cancelled = true;
            return;
        }

        // target already assigned
        if (target.Target != null)
            return;

        // no other humans to kill
        var allHumans = _mind.GetAliveHumans(args.MindId);
        if (allHumans.Count == 0)
        {
            args.Cancelled = true;
            return;
        }

        var allHeads = new HashSet<Entity<MindComponent>>();
        foreach (var person in allHumans)
        {
            if (TryComp<MindComponent>(person, out var mind) && mind.OwnedEntity is { } owned && HasComp<CommandStaffComponent>(owned) && !HasComp<NukeOperativeComponent>(owned))
                allHeads.Add(person); // Goob edit - exclude nuke ops from being selected as heads (bruh why would you even want that)
        } 

        // Goobstation - Cancel if there is no command staff
        if (allHeads.Count == 0)
        {
            args.Cancelled = true;
            return;
        }

        _target.SetTarget(ent.Owner, _random.Pick(allHeads), target);
    }

    private void OnRandomTraitorProgressAssigned(Entity<RandomTraitorProgressComponent> ent, ref ObjectiveAssignedEvent args)
    {
        // invalid prototype
        if (!TryComp<TargetObjectiveComponent>(ent.Owner, out var target))
        {
            args.Cancelled = true;
            return;
        }

        var traitors = _traitorRule.GetOtherTraitorMindsAliveAndConnected(args.Mind).ToHashSet();

        // cant help anyone who is tasked with helping:
        // 1. thats boring
        // 2. no cyclic progress dependencies!!!
        foreach (var traitor in traitors)
        {
            // TODO: replace this with TryComp<ObjectivesComponent>(traitor) or something when objectives are moved out of mind
            if (!TryComp<MindComponent>(traitor.Id, out var mind))
                continue;

            foreach (var objective in mind.Objectives)
            {
                if (HasComp<HelpProgressConditionComponent>(objective))
                    traitors.RemoveWhere(x => x.Mind == mind);
            }
        }

        // Can't have multiple objectives to help/save the same person
        foreach (var objective in args.Mind.Objectives)
        {
            if (HasComp<RandomTraitorAliveComponent>(objective) || HasComp<RandomTraitorProgressComponent>(objective))
            {
                if (TryComp<TargetObjectiveComponent>(objective, out var help))
                {
                    traitors.RemoveWhere(x => x.Id == help.Target);
                }
            }
        }

        // no more helpable traitors
        if (traitors.Count == 0)
        {
            args.Cancelled = true;
            return;
        }

        _target.SetTarget(ent.Owner, _random.Pick(traitors).Id, target);
    }

    private void OnRandomTraitorAliveAssigned(Entity<RandomTraitorAliveComponent> ent, ref ObjectiveAssignedEvent args)
    {
        // invalid prototype
        if (!TryComp<TargetObjectiveComponent>(ent.Owner, out var target))
        {
            args.Cancelled = true;
            return;
        }

        var traitors = _traitorRule.GetOtherTraitorMindsAliveAndConnected(args.Mind).ToHashSet();

        // Can't have multiple objectives to help/save the same person
        foreach (var objective in args.Mind.Objectives)
        {
            if (HasComp<RandomTraitorAliveComponent>(objective) || HasComp<RandomTraitorProgressComponent>(objective))
            {
                if (TryComp<TargetObjectiveComponent>(objective, out var help))
                {
                    traitors.RemoveWhere(x => x.Id == help.Target);
                }
            }
        }

        // You are the first/only traitor.
        if (traitors.Count == 0)
        {
            args.Cancelled = true;
            return;
        }

        _target.SetTarget(ent.Owner, _random.Pick(traitors).Id, target);
    }
}
