using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Goobstation.Shared.Spy;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Events;
using Content.Server.Objectives.Components;
using Content.Server.Objectives.Components.Targets;
using Content.Shared.Objectives;
using Content.Shared.Random;
using Content.Shared.Random.Helpers;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Spy;

/// <summary>
/// This handles...
/// </summary>
public sealed class SpyBountySystem : SharedSpyBountySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly IEntityManager _entityMan = default!;

    private readonly ProtoId<WeightedRandomPrototype> _weightedItemObjectives = "ThiefObjectiveGroupItem";
    private readonly ProtoId<WeightedRandomPrototype> _weightedStructureObjectives = "ThiefObjectiveGroupStructure";

    private const int GlobalBountyAmount = 10;

    public override void CreateDbEntity()
    {
        if (TryGetSpyDatabaseEntity(out var nullableEnt)) // if one exists dont spawn
            return;

        // spawn "database" entity
        var dbEnt = Spawn(null, MapCoordinates.Nullspace);
        EnsureComp<SpyBountyDatabaseComponent>(dbEnt);
        Log.Info("Spy DB Entity Created at UID: " + dbEnt.Id);
    }

    public override void SetupBounties()
    {
        if (!TryGetSpyDatabaseEntity(out var nullableEnt) || nullableEnt is not { } dbEnt)
            return;

        if (!TryPickBounties(out var bounties))
            return;

        dbEnt.Comp.Bounties = bounties.ToHashSet();

        foreach (var bounty in dbEnt.Comp.Bounties)
        {
            if (!_protoMan.TryIndex(bounty.TargetGroup, out var prototype))
                return;

            Log.Info(Loc.GetString(prototype.Name));
        }
    }

    private bool TryPickBounties([NotNullWhen(true)] out HashSet<SpyBountyData>? bounties)
    {
        bounties = null;

        if (!_protoMan.TryIndex(_weightedItemObjectives, out var weightedItemObjectives))
            return false;
        if (!_protoMan.TryIndex(_weightedStructureObjectives, out var weightedStructureObjectives))
            return false;

        List<EntProtoId> weightedPickedObjectives = [];

        // pick 20 objectives in total
        weightedPickedObjectives.AddRange(
            Enumerable.Range(0, GlobalBountyAmount).Select(_ => (EntProtoId) weightedItemObjectives.Pick(_random))
        );
        weightedPickedObjectives.AddRange(
            Enumerable.Range(0, GlobalBountyAmount).Select(_ => (EntProtoId) weightedStructureObjectives.Pick(_random))
        );

        // randomize them turn them inall uniqueto protoids and make sure they're .
        var randomizeTargets = weightedPickedObjectives
            .OrderBy(_ => _random.Next())
            .ToList()
            .Select(protoId => _protoMan.Index(protoId))
            .Distinct()
            .ToList();

        // make there StealConditionComponent easily accessible.
        List<(EntityPrototype proto, StealConditionComponent condition)> finalObjectives = [];
        foreach (var target in randomizeTargets)
        {
            if(!target.Components.TryGetComponent("StealCondition", out var stealConditionComponent) || stealConditionComponent is not StealConditionComponent stealCondition)
                continue;
            finalObjectives.Add((target, stealCondition));
        }

        // find there related uids.
        var targetQuery = EntityQueryEnumerator<StealTargetComponent>();

        List<Entity<StealTargetComponent>> possibleBounties = [];

        while (targetQuery.MoveNext(out var stealTarget, out var comp))
        {
            ProtoId<StealTargetGroupPrototype> stealGroup = comp.StealGroup;

            if(finalObjectives.Any(item => item.condition.StealGroup == stealGroup))
                possibleBounties.Add((stealTarget, comp));
        }

        bounties = possibleBounties
            .OrderBy(_ => _random.Next())
            .Take(GlobalBountyAmount)
            .Select(ent =>
                new SpyBountyData(GetNetEntity(ent), new ProtoId<StealTargetGroupPrototype>(ent.Comp.StealGroup)))
            .ToHashSet();

        return true;
    }

    // mary code v
    protected override bool TryGetSpyDatabaseEntity([NotNullWhen(true)] out Entity<SpyBountyDatabaseComponent>? entity)
    {
        entity = null;

        var query = EntityQueryEnumerator<SpyBountyDatabaseComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            entity = new Entity<SpyBountyDatabaseComponent>(uid, comp);
        }

        return entity is not null;
    }
}
