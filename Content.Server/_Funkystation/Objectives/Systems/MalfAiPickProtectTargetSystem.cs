using Content.Server.Objectives.Components;
using Content.Server.Roles;
using Content.Shared.Mind;
using Content.Shared.Roles;
using Content.Shared.Objectives.Components;
using Robust.Shared.Random;
using System.Linq;
using Content.Shared._Funkystation.MalfAI.Components;
using System.Diagnostics.CodeAnalysis;
using Content.Server.Objectives.Systems;
using Content.Server._Funkystation.Objectives.Components;

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

    private void OnObjectiveAssigned(Entity<MalfAiPickProtectTargetComponent> objective, ref ObjectiveAssignedEvent args)
    {
        if (!TryComp<MalfAiSabotageObjectiveComponent>(objective.Owner, out var sabotageComp))
            return;

        if (TryPickProtectTarget(args.MindId, out var target))
        {
            // Use standard target system instead of custom fields
            if (TryComp<TargetObjectiveComponent>(objective.Owner, out var targetComp))
                _target.SetTarget(objective.Owner, target, targetComp);
        }
    }

    private bool TryPickProtectTarget(Entity<MindComponent?> mind, [NotNullWhen(true)] out EntityUid picked)
    {
        picked = default;

        if (!Resolve(mind.Owner, ref mind.Comp))
            return false;

        if (mind.Comp.OwnedEntity is not { } mindOwner)
            return false;

        var candidates = new HashSet<EntityUid>();

        var query = EntityQueryEnumerator<MindComponent>();
        var minds = new HashSet<Entity<MindComponent>>();

        while (query.MoveNext(out var uid, out var mindComp))
        {
            if (mindComp.OwnedEntity is null)
                continue;

            if (mindComp.OwnedEntity == mindOwner)
                continue;

            minds.Add((uid, mindComp));
        }

        var traitorMinds = minds.Where(ent => _role.MindHasRole<TraitorRoleComponent>(ent.Owner)).ToHashSet();

        foreach (var traitorMind in traitorMinds)
        {
            if (!_role.MindHasRole<TraitorRoleComponent>(traitorMind.Owner)
                || traitorMind.Comp.OwnedEntity is not { } traitor)
                continue;

            candidates.Add(traitor);
        }

        // Second priority: Find traitor targets from their objectives
        if (candidates.Count == 0)
        {
            foreach (var traitorMind in traitorMinds)
            {
                foreach (var obj in traitorMind.Comp.Objectives)
                {
                    if (!TryComp<TargetObjectiveComponent>(obj, out var targetComp)
                        || targetComp.Target is not { } target
                        || target == mindOwner)
                        continue;

                    candidates.Add(target);
                }
            }
        }

        // Fallback: any crew member
        if (candidates.Count == 0)
            candidates = minds.Select(ent => ent.Owner).ToHashSet();

        if (candidates.Count > 0)
        {
            picked = _random.Pick(candidates);
            return true;
        }

        return false;
    }
}
