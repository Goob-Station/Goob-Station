// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.SlaughterDemon.Objectives;
using Content.Server.Objectives.Components;
using Content.Server.Objectives.Systems;
using Content.Shared.Humanoid;
using Content.Shared.Objectives.Components;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.SlaughterDemon.Objectives;

/// <summary>
/// A lot of the objectives are fluff. The actual ones are listed in this system.
/// </summary>
public sealed class SlaughterDemonObjectiveSystem : EntitySystem
{
    [Dependency] private readonly NumberObjectiveSystem _number = default!;

    private EntityQuery<ActorComponent> _actorQuery = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        _actorQuery = GetEntityQuery<ActorComponent>();

        SubscribeLocalEvent<SlaughterDevourConditionComponent, ObjectiveGetProgressEvent>(OnGetDevourProgress);
        SubscribeLocalEvent<SlaughterKillEveryoneConditionComponent, ObjectiveGetProgressEvent>(OnGetKillEveryoneProgress);
    }

    private void OnGetKillEveryoneProgress(Entity<SlaughterKillEveryoneConditionComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        args.Progress = Progress(ent.Comp.Devoured, GetAllPlayers());
    }

    private void OnGetDevourProgress(Entity<SlaughterDevourConditionComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        args.Progress = Progress(ent.Comp.Devour, _number.GetTarget(ent.Owner));
    }

    private static float Progress(int recruited, int target)
    {
        // prevent divide-by-zero
        if (target == 0)
            return 1f;

        return MathF.Min(recruited / (float) target, 1f);
    }

    private int GetAllPlayers()
    {
        var count = 0;

        var query = EntityQueryEnumerator<HumanoidAppearanceComponent>();
        while (query.MoveNext(out var uid, out _))
        {
            if (!_actorQuery.HasComp(uid))
                continue;

            count++;
        }

        return count;
    }
}
