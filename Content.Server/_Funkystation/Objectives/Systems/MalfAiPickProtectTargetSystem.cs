// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using System.Linq;
using Content.Server.Objectives.Components;
using Content.Server._Funkystation.Objectives.Components;
using Content.Server.Roles;
using Content.Shared._Funkystation.MalfAI;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;
using Content.Shared.Roles;
using Robust.Shared.Random;

using Content.Server.Objectives.Systems;

namespace Content.Server._Funkystation.Objectives.Systems;

/// <summary>
/// Handles protect target selection for Malf AI objectives, prioritizing traitors and traitor targets.
/// </summary>
public sealed class MalfAiPickProtectTargetSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedRoleSystem _role = default!;
    [Dependency] private readonly TargetObjectiveSystem _target = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MalfAiPickProtectTargetComponent, ObjectiveAssignedEvent>(OnObjectiveAssigned);
    }

    private void OnObjectiveAssigned(EntityUid uid, MalfAiPickProtectTargetComponent comp, ref ObjectiveAssignedEvent args)
    {
        if (!HasComp<MalfAiSabotageObjectiveComponent>(uid))
            return;

        if (TryPickProtectTarget(args.MindId, out var target))
        {
            // Use standard target system instead of custom fields
            if (TryComp<TargetObjectiveComponent>(uid, out var targetComp))
                _target.SetTarget(uid, target, targetComp);
        }
    }

    private bool TryPickProtectTarget(EntityUid mind, out EntityUid picked)
    {
        picked = default;

        var candidates = new List<EntityUid>();
        var mindOwner = Comp<MindComponent>(mind).OwnedEntity;

        // First priority: Find traitors
        var traitorMinds = EntityQuery<MindComponent>()
            .Where(m => m.OwnedEntity != null &&
                       (mindOwner == null || m.OwnedEntity != mindOwner) &&
                       _role.MindHasRole<TraitorRoleComponent>(m.Owner))
            .ToList();

        foreach (var traitorMind in traitorMinds)
        {
            if (traitorMind.OwnedEntity != null)
                candidates.Add(traitorMind.OwnedEntity.Value);
        }

        // Second priority: Find traitor targets from their objectives
        if (candidates.Count == 0)
        {
            var traitorTargets = new HashSet<EntityUid>();

            foreach (var traitorMind in traitorMinds)
            {
                foreach (var obj in traitorMind.Objectives)
                {
                    if (TryComp<TargetObjectiveComponent>(obj, out var targetComp) && targetComp.Target != null)
                        traitorTargets.Add(targetComp.Target.Value);
                }
            }

            candidates.AddRange(traitorTargets.Where(t => mindOwner == null || t != mindOwner.Value));
        }

        // Fallback: any crew member
        if (candidates.Count == 0)
        {
            var allPlayers = EntityQuery<MindComponent>()
                .Where(m => m.OwnedEntity != null && (mindOwner == null || m.OwnedEntity != mindOwner))
                .Select(m => m.OwnedEntity!.Value)
                .ToList();
            candidates.AddRange(allPlayers);
        }

        if (candidates.Count > 0)
        {
            picked = _random.Pick(candidates);
            return true;
        }

        return false;
    }
}
