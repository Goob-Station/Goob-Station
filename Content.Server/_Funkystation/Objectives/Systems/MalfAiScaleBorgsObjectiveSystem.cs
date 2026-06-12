// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Content.Server.Objectives.Components;
using Content.Server._Funkystation.Objectives.Components;
using Content.Shared._Funkystation.MalfAI;
using Content.Shared.Mind.Components;
using Content.Shared.Objectives.Components;
using Robust.Server.Player;

using Content.Server.Objectives.Systems;

namespace Content.Server._Funkystation.Objectives.Systems;

/// <summary>
/// Handles scaling the required number of borgs based on player count for Malf AI objectives.
/// </summary>
public sealed class MalfAiScaleBorgsObjectiveSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MalfAiScaleBorgsObjectiveComponent, ObjectiveGetProgressEvent>(OnGetProgress);
        SubscribeLocalEvent<MalfAiScaleBorgsObjectiveComponent, ObjectiveAfterAssignEvent>(OnAfterAssign);
    }

    private void OnAfterAssign(EntityUid uid, MalfAiScaleBorgsObjectiveComponent component, ref ObjectiveAfterAssignEvent args)
    {
        // Calculate target based on current player count
        var playerCount = _playerManager.PlayerCount;
        var targetBorgs = Math.Max(component.MinBorgs, Math.Min(component.MaxBorgs,
            (int) Math.Ceiling((float) playerCount / component.PlayersPerBorg)));

        component.Target = targetBorgs;

        // Update the objective name to include the number of borgs
        var name = Loc.GetString("malfai-objective-control-borgs", ("count", targetBorgs));
        _metaData.SetEntityName(uid, name);
    }

    private void OnGetProgress(EntityUid uid, MalfAiScaleBorgsObjectiveComponent component, ref ObjectiveGetProgressEvent args)
    {
        // Determine effective target for this calculation without mutating the component
        var effectiveTarget = component.Target > 0 ? component.Target : component.MinBorgs;

        // Count cyborgs controlled by this AI mind
        var controlledCount = 0;
        var query = AllEntityQuery<MalfAiControlledComponent>();
        while (query.MoveNext(out _, out var controlled))
        {
            if (controlled.Controller == null)
                continue;

            // Check if the controller matches the objective's mind directly
            if (TryComp<MindContainerComponent>(controlled.Controller.Value, out var controllerMind) &&
                controllerMind.Mind == args.MindId)
            {
                controlledCount++;
            }
        }

        // Calculate progress as controlled borgs / target borgs
        args.Progress = effectiveTarget > 0 ? Math.Min(1.0f, (float) controlledCount / effectiveTarget) : 1.0f;
    }
}
