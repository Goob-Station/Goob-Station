// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
// SPDX-FileCopyrightText: 2025 Timfa <timfalken@hotmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Threading;
using System.Threading.Tasks;
using Content.Goobstation.Shared.Silicon.Bots;
using Content.Server.Botany.Components;
using Content.Server.NPC;
using Content.Server.NPC.HTN.PrimitiveTasks;
using Content.Server.NPC.Pathfinding;
using Content.Shared.Emag.Components;
using Content.Shared.Interaction;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.NPC.HTN.PrimitiveTasks.Operators.Specific;

public sealed partial class PickNearbyServicableHydroponicsTrayOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    private EntityLookupSystem _lookup = default!;
    private PathfindingSystem _pathfinding = default!;

    /// <summary>
    /// Determines how close the bot needs to be to service a tray
    /// </summary>
    [DataField]
    public string RangeKey = NPCBlackboard.PlantbotServiceRange;

    /// <summary>
    /// Target entity to service
    /// </summary>
    [DataField(required: true)]
    public string TargetKey = string.Empty;

    /// <summary>
    /// Target entitycoordinates to move to.
    /// </summary>
    [DataField(required: true)]
    public string TargetMoveKey = string.Empty;

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);

        _lookup = sysManager.GetEntitySystem<EntityLookupSystem>();
        _pathfinding = sysManager.GetEntitySystem<PathfindingSystem>();
    }

    public override async Task<(bool Valid, Dictionary<string, object>? Effects)> Plan(NPCBlackboard blackboard,
        CancellationToken cancelToken)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

        if (!blackboard.TryGetValue<float>(RangeKey, out var range, _entManager) || !_entManager.TryGetComponent<PlantbotComponent>(owner, out _))
            return (false, null);

        var entityQuery = _entManager.GetEntityQuery<PlantHolderComponent>();
        var emagged = _entManager.HasComponent<EmaggedComponent>(owner);

        foreach (var target in _lookup.GetEntitiesInRange(owner, range))
        {
            if (!entityQuery.TryGetComponent(target, out var plantHolderComponent))
                continue;

            if (plantHolderComponent is { WaterLevel: >= PlantbotServiceOperator.RequiredWaterLevelToService, WeedLevel: <= PlantbotServiceOperator.RequiredWeedsAmountToWeed } && (!emagged || plantHolderComponent.Dead || plantHolderComponent.WaterLevel <= 0f))
                continue;

            //Needed to make sure it doesn't sometimes stop right outside it's interaction range
            var pathRange = SharedInteractionSystem.InteractionRange - 1f;
            var path = await _pathfinding.GetPath(owner, target, pathRange, cancelToken);

            if (path.Result == PathResult.NoPath)
                continue;

            return (true, new Dictionary<string, object>()
            {
                {TargetKey, target},
                {TargetMoveKey, _entManager.GetComponent<TransformComponent>(target).Coordinates},
                {NPCBlackboard.PathfindKey, path},
            });
        }

        return (false, null);
    }
}
