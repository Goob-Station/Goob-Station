using System.Threading;
using System.Threading.Tasks;
using Content.Goobstation.Shared.Nutrition.EntitySystems;
using Content.Goobstation.Shared.Xenobiology;
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

public sealed partial class PickCorpseEaterTargetOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _ent = default!;
    private NpcFactionSystem _factions = default!;
    private GoobHungerSystem _hunger = default!;
    private PathfindingSystem _pathfinding = default!;
    private EatCorpseSystem _eatCorpse = default!;

    /// <summary>
    /// Range in which we find target.
    /// </summary>
    [DataField(required: true)]
    public string RangeKey = string.Empty;

    /// <summary>
    /// Target coordinates.
    /// </summary>
    [DataField(required: true)]
    public string TargetKey = string.Empty;

    /// <summary>
    /// Target entity.
    /// </summary>
    [DataField]
    public string CorpseKey = string.Empty;

    /// <summary>
    /// Where the pathfinding result will be stored (if applicable). This gets removed after execution.
    /// </summary>
    [DataField]
    public string PathfindKey = NPCBlackboard.PathfindKey;

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);
        _pathfinding = sysManager.GetEntitySystem<PathfindingSystem>();
        _factions = sysManager.GetEntitySystem<NpcFactionSystem>();
        _hunger = sysManager.GetEntitySystem<GoobHungerSystem>();
        _eatCorpse = sysManager.GetEntitySystem<EatCorpseSystem>();
    }

    public override async Task<(bool Valid, Dictionary<string, object>? Effects)> Plan(NPCBlackboard blackboard, CancellationToken cancelToken)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);
        var targets = new List<EntityUid>();

        if (!blackboard.TryGetValue<float>(RangeKey, out var range, _ent)
        || !_ent.TryGetComponent<CorpseEaterComponent>(owner, out var eaterComp)
        || _hunger.IsHungerAboveState(owner, HungerThreshold.Peckish)) // eat corpses only if very hungry
            return (false, null);

        foreach (var entity in _factions.GetNearbyHostiles(owner, range))
        {
            if (!_eatCorpse.CanEatCorpse(owner, entity, eaterComp))
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
                { CorpseKey, target },
                { PathfindKey, path },
            });
        }

        return (false, null);
    }
}
