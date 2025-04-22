using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Goobstation.Shared.Spy;
using Content.Server.Objectives.Components;
using Content.Server.Objectives.Components.Targets;
using Content.Shared.FixedPoint;
using Content.Shared.Objectives;
using Content.Shared.Random.Helpers;
using Content.Shared.Store;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Spy;

public sealed partial class SpySystem
{
    private readonly HashSet<ProtoId<StoreCategoryPrototype>> _categories =
    [
        "UplinkWeaponry",
        "UplinkAmmo",
        "UplinkExplosives",
        "UplinkChemicals",
        "UplinkDeception",
        "UplinkDisruption",
        "UplinkImplants",
        "UplinkAllies",
        "UplinkWearables",
        "UplinkJob",
        "UplinkPointless",
        "UplinkSales",
    ];

    public override void CreateDbEntity()
    {
        if (TryGetSpyDatabaseEntity(out var nullableEnt)) // if one exists dont spawn
            return;

        // spawn "database" entity
        var dbEnt = Spawn(null, MapCoordinates.Nullspace);
        EnsureComp<SpyBountyDatabaseComponent>(dbEnt);
        Log.Info("Spy DB Entity Created at UID: " + dbEnt.Id);
    }

    private bool TrySetBountyClaimed(NetEntity bountyEntity, [NotNullWhen(true)] out SpyBountyData? bountyData)
    {
        bountyData = null;
        if (!TryGetSpyDatabaseEntity(out var nullableEnt) || nullableEnt is not { } dbEnt)
            return false;

        var bounty = dbEnt.Comp.Bounties.FirstOrDefault(b => b.TargetEntity == bountyEntity);
        if (bounty == null)
            return false;
        // goida event

        bounty.Claimed = true;
        bountyData = bounty;
        return true;
    }

    public override void SetupBounties()
    {
        if (!TryGetSpyDatabaseEntity(out var nullableEnt) || nullableEnt is not { } dbEnt)
            return;

        if (!TryPickBounties(out var bounties))
            return;

        dbEnt.Comp.Bounties = bounties.ToList();

        foreach (var bounty in dbEnt.Comp.Bounties)
        {
            if (!_protoMan.TryIndex(bounty.TargetGroup, out var prototype))
                return;

            Log.Info(Loc.GetString(prototype.Name));
        }
    }

    private bool TryPickBounties([NotNullWhen(true)] out List<SpyBountyData>? bounties)
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
            .Distinct()
            .Select(protoId => _protoMan.Index(protoId))
            .OrderBy(_ => _random.Next())
            .ToList();

        // make there StealConditionComponent easily accessible.
        List<(EntityPrototype proto, StealConditionComponent condition)> finalObjectives = [];
        foreach (var target in randomizeTargets)
        {
            if (!target.Components.TryGetComponent("StealCondition", out var stealConditionComponent) ||
                stealConditionComponent is not StealConditionComponent stealCondition)
                continue;
            finalObjectives.Add((target, stealCondition));
        }

        // find there related uids.
        var targetQuery = EntityQueryEnumerator<StealTargetComponent>();

        List<Entity<StealTargetComponent>> possibleBounties = [];

        while (targetQuery.MoveNext(out var stealTarget, out var comp))
        {
            ProtoId<StealTargetGroupPrototype> stealGroup = comp.StealGroup;

            if (finalObjectives.Any(item => item.condition.StealGroup == stealGroup))
                possibleBounties.Add((stealTarget, comp));
        }

        // select a random reward
        // test

        var listings = _store.GetAllListings()
            .OrderBy(p =>
                p.Cost.Values.Sum())
            .Where(p =>
                p.Categories.Any(category =>
                    _categories.Contains(category)))
            .ToList();

        bounties = possibleBounties
            .DistinctBy(ent => ent.Comp.StealGroup)
            .DistinctBy(ent => ent.Owner)
            .Take(GlobalBountyAmount)
            .Select(ent =>
                new SpyBountyData(GetNetEntity(ent),
                    new ProtoId<StealTargetGroupPrototype>(ent.Comp.StealGroup),
                    listings
                        .OrderBy(_ => _random.Next())
                        .First())) // select a random item
            .OrderBy(_ => _random.Next())
            .ToList();

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
