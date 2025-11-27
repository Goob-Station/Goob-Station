// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Objectives.Components;
using Content.Shared._Funkystation.MalfAI.Components;
using Content.Shared.Objectives.Components;

namespace Content.Server._Funkystation.Objectives.Systems;

/// <summary>
/// System that handles malfunction AI sabotage objectives.
/// </summary>
public sealed class MalfAiSabotageObjectiveSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MalfAiSabotageObjectiveComponent, ObjectiveAssignedEvent>(OnObjectiveAssigned);
    }

    private void OnObjectiveAssigned(EntityUid uid, MalfAiSabotageObjectiveComponent comp, ref ObjectiveAssignedEvent args)
    {
        // Debug: If no target was assigned (e.g., in single player), auto-complete for testing
        if (!TryComp<TargetObjectiveComponent>(uid, out var targetObj)
            || targetObj.Target is not null)
            return;

        Log.Warning($"MalfAi {comp.SabotageType} objective {ToPrettyString(uid)} could not find a target. This may be due to insufficient players.");
    }
}
