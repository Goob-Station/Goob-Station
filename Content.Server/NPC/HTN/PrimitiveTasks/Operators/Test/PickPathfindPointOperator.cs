// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Threading;
using System.Threading.Tasks;
using Content.Server.NPC.Components;
using Robust.Shared.Random;

namespace Content.Server.NPC.HTN.PrimitiveTasks.Operators.Test;

public sealed partial class PickPathfindPointOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override async Task<(bool Valid, Dictionary<string, object>? Effects)> Plan(NPCBlackboard blackboard,
        CancellationToken cancelToken)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

        // Find all pathfind points on the same grid and choose to move to it.
        var xform = _entManager.GetComponent<TransformComponent>(owner);
        var gridUid = xform.GridUid;

        if (gridUid == null)
            return (false, null);

        var points = new List<TransformComponent>();

        foreach (var (point, pointXform) in _entManager.EntityQuery<NPCPathfindPointComponent, TransformComponent>(true))
        {
            if (gridUid != pointXform.GridUid)
                continue;

            points.Add(pointXform);
        }

        if (points.Count == 0)
            return (false, null);

        var selected = _random.Pick(points);

        return (true, new Dictionary<string, object>()
        {
            { NPCBlackboard.MovementTarget, selected.Coordinates }
        });
    }
}