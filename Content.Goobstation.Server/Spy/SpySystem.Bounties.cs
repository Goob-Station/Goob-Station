using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Goobstation.Shared.Spy;
using Content.Server.Objectives.Components;
using Content.Server.Objectives.Components.Targets;
using Content.Server.Station.Components;
using Content.Shared.FixedPoint;
using Content.Shared.Objectives;
using Content.Shared.Random;
using Content.Shared.Random.Helpers;
using Content.Shared.Station.Components;
using Content.Shared.Store;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Spy;

public sealed partial class SpySystem
{
    public override void CreateDbEntity()
    {
        if (TryGetSpyDatabaseEntity(out var nullableEnt)) // if one exists dont spawn
            return;

        // spawn "database" entity
        var dbEnt = Spawn(null, MapCoordinates.Nullspace);
        var comp = EnsureComp<SpyBountyDatabaseComponent>(dbEnt);
        comp.NextReset = _timing.CurTime + comp.ResetTime; // unhardcode
        Log.Info("Spy DB Entity Created at UID: " + dbEnt.Id);
    }

    private void BountyUpdate()
    {
        if (!TryGetSpyDatabaseEntity(out var nullableEnt)
            || nullableEnt is not { } dbEnt)
            return;

        if (_timing.CurTime < dbEnt.Comp.NextReset)
            return;

        if(!TryPickBounties(out var bounties))
            return;

        dbEnt.Comp.Bounties = bounties;
        dbEnt.Comp.NextReset = _timing.CurTime + dbEnt.Comp.ResetTime;
    }

    private bool TrySetBountyClaimed(EntityUid uplink, EntityUid user, ProtoId<StealTargetGroupPrototype> stealGroup, [NotNullWhen(true)] out SpyBountyData? bountyData)
    {
        bountyData = null;
        if (!TryGetSpyDatabaseEntity(out var nullableEnt) || nullableEnt is not { } dbEnt)
            return false;

        // Find first unclaimed bounty matching the steal group
        bountyData = dbEnt.Comp.Bounties.FirstOrDefault(b =>
            b.StealGroup == stealGroup &&
            !b.Claimed);

        if (bountyData == null)
            return false;

        bountyData.Claimed = true;
        UpdateUserInterface(uplink, user);
        return true;
    }

    public override void SetupBounties()
    {
        if (!TryGetSpyDatabaseEntity(out var nullableEnt)
            || nullableEnt is not { } dbEnt
            || !TryPickBounties(out var bounties))
            return;

        dbEnt.Comp.Bounties = bounties.ToList();
    }

    private bool TryPickBounties([NotNullWhen(true)] out List<SpyBountyData>? bounties)
    {
        bounties = null;

        if (!_protoMan.TryIndex(_easyObjectives, out var easyObjectives)
            || !_protoMan.TryIndex(_mediumObjectives, out var mediumObjectives)
            || !_protoMan.TryIndex(_hardObjectives, out var hardObjectives))
            return false;

        var finalObjectives = GenerateObjectives(easyObjectives, mediumObjectives, hardObjectives);

        // Get all unique steal groups from objectives
        var stealGroups = finalObjectives
            .Select(o => o.Condition.StealGroup)
            .Distinct()
            .Where(StealTargetExists)
            .ToList();

        // Shuffle for randomness
        _random.Shuffle(stealGroups);

        // Get possible rewards
        var listings = GetPossibleBountyRewards(out var easyObjects, out var mediumObjects, out var hardObjects);
        if (listings.Count == 0)
        {
            bounties = [];
            return false;
        }

        // Create bounties for each steal group
        bounties = stealGroups
            .Take(GlobalBountyAmount)
            .Select(stealGroup =>
            {
                // Find the original objective to get difficulty
                var objective = finalObjectives.First(o => o.Condition.StealGroup == stealGroup);

                var targetList = objective.Diff switch
                {
                    SpyBountyDifficulty.Easy => easyObjects,
                    SpyBountyDifficulty.Medium => mediumObjects,
                    SpyBountyDifficulty.Hard => hardObjects,
                    _ => throw new ArgumentOutOfRangeException(),
                };

                var randomListing = targetList[_random.Next(targetList.Count)];
                return new SpyBountyData(stealGroup, randomListing, objective.Diff);
            })
            .ToList();

        // Final shuffle
        _random.Shuffle(bounties);
        return true;
    }

    private bool TryGetMainStation([NotNullWhen(true)] out EntityUid? mainStation)
    {
        mainStation = null;
        AllEntityQuery<BecomesStationComponent, StationMemberComponent>().MoveNext(out var eqData, out _, out _);
        var station = _station.GetOwningStation(eqData);
        if (station is null)
            return false;

        mainStation = station;
        return true;
    }

    private bool StealTargetExists(ProtoId<StealTargetGroupPrototype> stealGroup)
    {
        if (!TryGetMainStation(out var mainStation))
            return false;

        var query = EntityQueryEnumerator<StealTargetComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.StealGroup == stealGroup && _station.GetOwningStation(uid) == mainStation.Value)
                return true;
        }
        return false;
    }

    private List<ListingData> GetPossibleBountyRewards(out List<ListingData> easyObjects, out List<ListingData> mediumObjects, out List<ListingData> hardObjects)
    {
        var listings = _store.GetAllListings()
            .OrderBy(p =>
                p.Cost.Values.Sum())
            .Where(p =>
                p.Categories.Any(category =>
                    _categories.Contains(category)))
            .ToList();

        easyObjects = listings
            .Where(p => p.Cost.Values.Sum() <= 25)
            .ToList();
        mediumObjects = listings
            .Where(p => p.Cost.Values.Sum() > 25 && p.Cost.Values.Sum() <= 50)
            .ToList();
        hardObjects = listings
            .Where(p => p.Cost.Values.Sum() > 50)
            .ToList();

        return listings;
    }

    private List<StealTarget> GenerateObjectives(WeightedRandomPrototype easyObjectives, WeightedRandomPrototype mediumObjectives, WeightedRandomPrototype hardObjectives)
    {
        var weightedPickedObjectives = new List<StealTargetId>();

        foreach (var (difficulty, weight) in _difficultyWeights)
        {
            var prototype = difficulty switch
            {
                SpyBountyDifficulty.Easy => easyObjectives,
                SpyBountyDifficulty.Medium => mediumObjectives,
                SpyBountyDifficulty.Hard => hardObjectives,
                _ => throw new ArgumentOutOfRangeException()
            };

            for (var i = 0; i < weight; i++)
            {
                weightedPickedObjectives.Add(new StealTargetId(
                     new EntProtoId(prototype.Pick(_random)),
                    difficulty
                ));
            }
        }

        List<(EntityPrototype proto, SpyBountyDifficulty diff)> randomizeTargets = weightedPickedObjectives
            .Distinct()
            .Select(obj => (_protoMan.Index(obj.Proto), obj.Diff))
            .ToList();

        _random.Shuffle(randomizeTargets);

        // make there StealConditionComponent easily accessible.
        List<StealTarget> finalObjectives = [];
        foreach (var target in randomizeTargets)
        {
            if (!target.proto.Components.TryGetComponent("StealCondition", out var stealConditionComponent) ||
                stealConditionComponent is not StealConditionComponent stealCondition)
                continue;

            var stealTarget = new StealTarget(target.proto, stealCondition, target.diff);
            finalObjectives.Add(stealTarget);
        }
        _random.Shuffle(finalObjectives);

        return finalObjectives;
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
