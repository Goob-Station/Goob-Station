// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Threading;
using System.Threading.Tasks;
using Content.Goobstation.Shared.Nutrition.EntitySystems;
using Content.Goobstation.Shared.Xenobiology.Components;
using Content.Server.NPC;
using Content.Server.NPC.HTN.PrimitiveTasks;
using Content.Server.NPC.Pathfinding;
using Content.Shared.Mobs.Systems;
using Content.Shared.NPC.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Whitelist;

namespace Content.Goobstation.Server.Xenobiology.HTN;

public sealed partial class PickSlimeLatchTargetOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _ent = default!;
    private NpcFactionSystem _factions = default!;
    private MobStateSystem _mobSystem = default!;
    private GoobHungerSystem _hunger = default!;
    private PathfindingSystem _pathfinding = default!;
    private SlimeLatchSystem _latch = default!;
    private EntityWhitelistSystem _whitelist = default!;

    private EntityQuery<BeingLatchedComponent> _latchedQuery = default!;
    private EntityQuery<SlimeDamageOvertimeComponent> _dotQuery = default!;
    private EntityQuery<SlimeComponent> _slimeQuery = default!;
    private EntityQuery<MobGrowthComponent> _growthQuery = default!;

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
        _whitelist = sysManager.GetEntitySystem<EntityWhitelistSystem>();

        _latchedQuery = _ent.GetEntityQuery<BeingLatchedComponent>();
        _dotQuery = _ent.GetEntityQuery<SlimeDamageOvertimeComponent>();
        _slimeQuery = _ent.GetEntityQuery<SlimeComponent>();
        _growthQuery = _ent.GetEntityQuery<MobGrowthComponent>();
    }

    public override async Task<(bool Valid, Dictionary<string, object>? Effects)> Plan(NPCBlackboard blackboard, CancellationToken cancelToken)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);
        var targets = new List<EntityUid>();

        if (!blackboard.TryGetValue<float>(RangeKey, out var range, _ent)
        || !_slimeQuery.TryComp(owner, out var slimeComp)
        || !_growthQuery.TryComp(owner, out var growthComp)
        || growthComp.IsFirstStage && _hunger.IsHungerAboveState(owner, HungerThreshold.Peckish) // babies only latch when very hungry
        || _latch.IsLatched((owner, slimeComp)))
            return (false, null);

        foreach (var entity in _factions.GetNearbyHostiles(owner, range))
        {
            if (_latchedQuery.HasComp(entity)
            || _dotQuery.HasComp(entity) // it's taken
            || _mobSystem.IsDead(entity)
            || growthComp.IsFirstStage && entity == slimeComp.Tamer) // no killing tamer
                continue;

            targets.Add(entity);
        }

        if (targets.Count > 0)
        {
            targets.Sort((x, y) => _whitelist.IsWhitelistPass(slimeComp.Whitelist, y)
            .CompareTo(_whitelist.IsWhitelistPass(slimeComp.Whitelist, x)));
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
