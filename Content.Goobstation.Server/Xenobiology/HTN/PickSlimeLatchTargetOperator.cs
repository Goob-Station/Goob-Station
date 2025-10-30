// SPDX-FileCopyrightText: 2024 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Threading;
using System.Threading.Tasks;
using Content.Goobstation.Shared.Nutrition.EntitySystems;
using Content.Goobstation.Shared.Xenobiology.Components;
using Content.Server.NPC;
using Content.Server.NPC.HTN.PrimitiveTasks;
using Content.Server.NPC.Pathfinding;
using Content.Shared.Humanoid;
using Content.Shared.Mobs.Systems;
using Content.Shared.NPC.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;

namespace Content.Goobstation.Server.Xenobiology.HTN;

public sealed partial class PickSlimeLatchTargetOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _ent = default!;
    private NpcFactionSystem _factions = default!;
    private MobStateSystem _mobSystem = default!;
    private GoobHungerSystem _hunger = default!;
    private PathfindingSystem _pathfinding = default!;
    private SlimeLatchSystem _latch = default!;

    [DataField(required: true)]
    public string RangeKey = string.Empty;

    [DataField(required: true)]
    public string TargetKey = string.Empty;

    [DataField]
    public string LatchKey = string.Empty;

    /// <summary>
    ///     Where the pathfinding result will be stored (if applicable). This gets removed after execution.
    /// </summary>
    [DataField]
    public string PathfindKey = NPCBlackboard.PathfindKey;

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);
        _pathfinding = sysManager.GetEntitySystem<PathfindingSystem>();
        _mobSystem = sysManager.GetEntitySystem<MobStateSystem>();
        _factions = sysManager.GetEntitySystem<NpcFactionSystem>();
        _hunger = sysManager.GetEntitySystem<GoobHungerSystem>();
        _latch = sysManager.GetEntitySystem<SlimeLatchSystem>();
    }

    public override async Task<(bool Valid, Dictionary<string, object>? Effects)> Plan(NPCBlackboard blackboard, CancellationToken cancelToken)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);
        var targets = new List<EntityUid>();

        if (!blackboard.TryGetValue<float>(RangeKey, out var range, _ent)
        || !_ent.TryGetComponent<SlimeComponent>(owner, out var slimeComp)
        || !_ent.TryGetComponent<MobGrowthComponent>(owner, out var growthComp)
        || (growthComp.IsFirstStage && _hunger.IsHungerAboveState(owner, HungerThreshold.Peckish)) // babies only latch when very hungry
        || _latch.IsLatched((owner, slimeComp)))
            return (false, null);

        foreach (var entity in _factions.GetNearbyHostiles(owner, range))
        {
            if (_ent.HasComponent<BeingLatchedComponent>(entity)
            || _ent.HasComponent<SlimeDamageOvertimeComponent>(entity) // it's taken
            || _mobSystem.IsDead(entity)
            || (growthComp.IsFirstStage && entity == slimeComp.Tamer) // no killing tamer
            || (entity == slimeComp.Tamer && _hunger.IsHungerAboveState(owner, HungerThreshold.Peckish))) // no killing tamer unless very hungry
                continue;

            targets.Add(entity);
        }

        foreach (var target in targets)
        {
            if (!_ent.TryGetComponent<TransformComponent>(target, out var xform))
                continue;

            var targetCoords = xform.Coordinates;
            var path = await _pathfinding.GetPath(owner, target, range, cancelToken);

            if (path.Result != PathResult.Path)
                continue;

            return (true, new Dictionary<string, object>()
            {
                { TargetKey, targetCoords },
                { LatchKey, target },
                { PathfindKey, path },
            });
        }

        return (false, null);
    }
}
