// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Objectives.Systems;
using Content.Shared._Gabystation.MalfAi.Components;
using Content.Shared._Funkystation.MalfAI.Components;
using Content.Shared._Funkystation.MalfAI.Events;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;

namespace Content.Server._Funkystation.Objectives.Systems;

/// <summary>
/// Handles completion logic for Malf AI objectives.
/// </summary>
public sealed class MalfAiObjectiveSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly TargetObjectiveSystem _target = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MalfAiDoomsdayCompletedEvent>(OnDoomsdayCompleted);

        SubscribeLocalEvent<MalfAiSabotageObjectiveComponent, ObjectiveGetProgressEvent>(OnSabotageGetProgress);
        SubscribeLocalEvent<MalfAiSurviveObjectiveComponent, ObjectiveGetProgressEvent>(OnSurviveGetProgress);
    }

    private void OnDoomsdayCompleted(MalfAiDoomsdayCompletedEvent ev)
    {
        EnsureComp<MalfAiDoomsdayCompletedComponent>(ev.Ai);
    }

    private void OnSabotageGetProgress(Entity<MalfAiSabotageObjectiveComponent> objective, ref ObjectiveGetProgressEvent args)
    {
        var comp = objective.Comp;
        if (comp.SabotageType == MalfAiSabotageType.Doomsday)
        {
            // Progress is 1.0 if doomsday has been completed for this AI
            if (args.Mind.OwnedEntity is { } ent
                && HasComp<MalfAiDoomsdayCompletedComponent>(ent))
                args.Progress = 1.0f;
            else
                args.Progress = 0.0f;
        }
        else if (comp.SabotageType == MalfAiSabotageType.Assassinate)
        {
            // Use standard target checking - if target doesn't exist or is dead, objective complete
            if (!_target.GetTarget(objective, out var target))
            {
                args.Progress = 0.0f;
                return;
            }

            args.Progress = GetKillProgress(target.Value);
        }
        else if (comp.SabotageType == MalfAiSabotageType.Protect)
        {
            // Use standard target checking - if target doesn't exist, objective failed
            if (!_target.GetTarget(objective, out var target))
            {
                args.Progress = 0.0f;
                return;
            }

            args.Progress = GetProtectProgress(target.Value);
        }
    }

    private void OnSurviveGetProgress(Entity<MalfAiSurviveObjectiveComponent> objective, ref ObjectiveGetProgressEvent args)
    {
        if (args.Mind.OwnedEntity is { } ent
            && HasComp<MalfunctioningAiComponent>(ent))
            args.Progress = 1.0f;
        else
            args.Progress = 0.0f;
    }

    private float GetKillProgress(EntityUid target)
    {
        // deleted or gibbed or something, counts as dead
        if (!TryComp<MindComponent>(target, out var mind) || mind.OwnedEntity == null)
            return 1f;

        // dead is success
        if (_mind.IsCharacterDeadIc(mind))
            return 1f;

        return 0f;
    }

    private float GetProtectProgress(EntityUid target)
    {
        // deleted or gibbed, objective failed
        if (!TryComp<MindComponent>(target, out var mind) || mind.OwnedEntity == null)
            return 0f;

        // dead is failure
        if (_mind.IsCharacterDeadIc(mind))
            return 0f;

        return 1f;
    }

}
