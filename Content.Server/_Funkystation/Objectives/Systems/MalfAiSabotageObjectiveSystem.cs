// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Content.Server.Objectives.Components;
using Content.Server._Funkystation.Objectives.Components;
using Content.Shared._Funkystation.MalfAI;
using Content.Shared.Objectives.Components;

using Content.Server.Objectives.Systems;

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
        // If no target was assigned (e.g., in single player), log a warning for visibility.
        if (TryComp<TargetObjectiveComponent>(uid, out var targetObj) && targetObj.Target == null)
        {
            Log.Warning($"MalfAi {comp.SabotageType} objective {ToPrettyString(uid)} could not find a target. This may be due to insufficient players.");
        }
    }
}
