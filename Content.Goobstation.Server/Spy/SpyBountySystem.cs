using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Goobstation.Shared.Spy;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Events;
using Content.Server.Objectives.Components.Targets;
using Content.Shared.Objectives;
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

    private const int GlobalBountyAmount = 5;

    public override void CreateDbEntity()
    {
        if (TryGetSpyDatabaseEntity(out var nullableEnt)) // if one exists dont spawn
            return;

        // spawn "database" entity
        var dbEnt = Spawn(null, MapCoordinates.Nullspace);
        EnsureComp<SpyBountyDatabaseComponent>(dbEnt);
    }

    public override void SetupBounties()
    {
        if (!TryGetSpyDatabaseEntity(out var nullableEnt) || nullableEnt is not { } dbEnt)
            return;

        dbEnt.Comp.Bounties = PickBounties();

        foreach (var bounty in dbEnt.Comp.Bounties)
        {
            if (!_protoMan.TryIndex(bounty.TargetGroup, out var prototype))
                return;

            Log.Info(Loc.GetString(prototype.Name));
        }
    }

    private List<SpyBounty> PickBounties()
    {
        var targetQuery = EntityQueryEnumerator<StealTargetComponent>();

        List<Entity<StealTargetComponent>> possibleBounties = [];

        while (targetQuery.MoveNext(out var stealTarget, out var comp))
        {
            possibleBounties.Add((stealTarget, comp));
        }

        return possibleBounties
            .OrderBy(_ => _random.Next())
            .Take(GlobalBountyAmount)
            .Select(ent =>
                new SpyBounty(ent.Owner, (ProtoId<StealTargetGroupPrototype>) ent.Comp.StealGroup)) // cursed linq
            .ToList();
    }

    // mary code v
    private bool TryGetSpyDatabaseEntity([NotNullWhen(true)] out Entity<SpyBountyDatabaseComponent>? entity)
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
