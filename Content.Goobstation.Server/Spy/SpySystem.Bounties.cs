using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Goobstation.Shared.Spy;
using Content.Server.Objectives.Components;
using Content.Server.Objectives.Components.Targets;
using Content.Shared.FixedPoint;
using Content.Shared.Objectives;
using Content.Shared.Random;
using Content.Shared.Random.Helpers;
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

    private bool TrySetBountyClaimed(EntityUid uplink, EntityUid user, NetEntity bountyEntity, [NotNullWhen(true)] out SpyBountyData? bountyData)
    {
        bountyData = null;
        if (!TryGetSpyDatabaseEntity(out var nullableEnt)
            || nullableEnt is not { } dbEnt)
            return false;

        var bounty = dbEnt.Comp.Bounties.FirstOrDefault(b => b.TargetEntity == bountyEntity);
        if (bounty == null)
            return false;
        // goida event

        bounty.Claimed = true;
        bountyData = bounty;
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
        // find there related uids.
        var targetQuery = EntityQueryEnumerator<StealTargetComponent>();

        List<PossibleBounty> possibleBounties = [];

        while (targetQuery.MoveNext(out var stealTarget, out var comp))
        {
            ProtoId<StealTargetGroupPrototype> stealGroup = comp.StealGroup;

            var match = finalObjectives.FirstOrDefault(item => item.Condition.StealGroup == stealGroup);

            if (match == default)
                continue;
            possibleBounties.Add(new PossibleBounty((stealTarget, comp), match.Diff));
        }

        // select a random reward
        var listings = GetPossibleBountyRewards(out var easyObjects, out var mediumObjects, out var hardObjects);

        if (listings.Count == 0)
        {
            bounties = [];
            return false;
        }

        var seenStealGroups = new HashSet<ProtoId<StealTargetGroupPrototype>>();
        var seenOwners = new HashSet<EntityUid>();

        var bountyList = possibleBounties
            .Where(ent =>
                seenStealGroups.Add((ProtoId<StealTargetGroupPrototype>) ent.Ent.Comp.StealGroup) &&
                seenOwners.Add(ent.Ent.Owner))
            .Take(GlobalBountyAmount)
            .Select(ent =>
            {
                var targetList = ent.Diff switch
                {
                    SpyBountyDifficulty.Easy => easyObjects,
                    SpyBountyDifficulty.Medium => mediumObjects,
                    SpyBountyDifficulty.Hard => hardObjects,
                    _ => throw new ArgumentOutOfRangeException(),
                };

                var randomListing = targetList[_random.Next(targetList.Count)];
                return new SpyBountyData(
                    GetNetEntity(ent.Ent),
                    new ProtoId<StealTargetGroupPrototype>(ent.Ent.Comp.StealGroup),
                    randomListing,
                    ent.Diff);
            })
            .ToList();

        // durk randomize
        for (var i = bountyList.Count - 1; i > 0; i--)
        {
            var j = _random.Next(i + 1);
            (bountyList[i], bountyList[j]) = (bountyList[j], bountyList[i]);
        }

        bounties = bountyList;

        return true;
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
        List<StealTargetId> weightedPickedObjectives = [];

        weightedPickedObjectives.AddRange(
            Enumerable.Repeat(SpyBountyDifficulty.Easy, 5)
                .Concat(Enumerable.Repeat(SpyBountyDifficulty.Medium, 4))
                .Concat(Enumerable.Repeat(SpyBountyDifficulty.Hard, 3))
                .Select((difficulty, _) =>
                    new StealTargetId(
                        (EntProtoId) (difficulty == SpyBountyDifficulty.Easy ? easyObjectives :
                            difficulty == SpyBountyDifficulty.Medium ? mediumObjectives : hardObjectives).Pick(_random),
                        difficulty)
                )
        );

        // randomize them turn them inall uniqueto protoids and make sure they're .
        List<(EntityPrototype proto, SpyBountyDifficulty diff)> randomizeTargets = weightedPickedObjectives
            .Distinct()
            .Select(obj => (_protoMan.Index(obj.Proto), obj.Diff))
            .ToList();

        // durkle randomize
        for (var i = randomizeTargets.Count - 1; i > 0; i--)
        {
            var j = _random.Next(i + 1);
            (randomizeTargets[i], randomizeTargets[j]) = (randomizeTargets[j], randomizeTargets[i]);
        }

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
