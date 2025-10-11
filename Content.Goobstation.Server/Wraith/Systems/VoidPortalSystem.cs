using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Components.Mobs;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using System.Linq;

namespace Content.Goobstation.Server.Wraith.Systems;

public sealed class VoidPortalSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VoidPortalComponent, MapInitEvent>(OnMapInit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<VoidPortalComponent>();
        while (query.MoveNext(out var uid, out var portal))
        {
            if (_timing.CurTime < portal.Accumulator)
                continue;

            UpdatePortal(uid, portal);
        }
    }

    private void OnMapInit(Entity<VoidPortalComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.CurrentPower = ent.Comp.ExtraPower; // start with initial power
        TrySpawn(ent.Owner, ent);

        ent.Comp.Accumulator = _timing.CurTime + ent.Comp.SpawnInterval;
    }

    private void UpdatePortal(EntityUid uid, VoidPortalComponent portal)
    {
        portal.Accumulator = _timing.CurTime + portal.SpawnInterval;

        // --- Wave Power Growth ---
        var gained = portal.PowerGainPerTick + (portal.ExtraPower * portal.WavesCompleted);
        portal.CurrentPower = Math.Min(portal.CurrentPower + gained, portal.MaxPower);

        portal.WavesCompleted++;

        TrySpawn(uid, portal);
    }

    private void TrySpawn(EntityUid uid, VoidPortalComponent portal)
    {
        var transform = Transform(uid);
        var grid = transform.GridUid;
        var center = transform.Coordinates;

        // Determine spawn coordinates with offset
        var spawnCoords = grid != null
            ? new EntityCoordinates(
                grid.Value,
                center.X + _random.Next(-portal.OffsetForSpawn, portal.OffsetForSpawn + 1),
                center.Y + _random.Next(-portal.OffsetForSpawn, portal.OffsetForSpawn + 1)
              )
            : center;

        // Count alive summoned mobs nearby
        var nearbyEntities = _lookup.GetEntitiesInRange(spawnCoords, portal.SearchRange);
        nearbyEntities.RemoveWhere(e => !HasComp<VoidSummonedComponent>(e));
        int aliveSummonedCount = nearbyEntities.Count(e => !_mobState.IsDead(e));

        // Decide what to spawn
        EntProtoId protoToSpawn;

        if (aliveSummonedCount >= portal.MaxEntitiesAlive || portal.MobEntries.Count == 0)
        {
            protoToSpawn = portal.EmptyPortal;
        }
        else
        {
            // Pick a random entry weighted by cost/power availability
            var affordable = portal.MobEntries.Where(e => e.Cost <= portal.CurrentPower).ToList();
            if (affordable.Count == 0)
            {
                protoToSpawn = portal.EmptyPortal;
            }
            else
            {
                var entry = _random.Pick(affordable);
                protoToSpawn = entry.Prototype;
                portal.CurrentPower -= entry.Cost;
            }
        }

        Spawn(protoToSpawn, spawnCoords);
    }
}
