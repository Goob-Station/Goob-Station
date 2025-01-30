using System.Linq;
using Content.Server._Lavaland.Tendril.Components;
using Content.Shared.Destructible;
using Content.Shared.Mobs;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Lavaland.Tendril;

public sealed class TendrilSystem : EntitySystem
{
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IGameTiming _time = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TendrilComponent, TendrilMobDeadEvent>(OnTendrilMobDeath);
        SubscribeLocalEvent<TendrilComponent, DestructionEventArgs>(OnTendrilDestruction);
        SubscribeLocalEvent<TendrilComponent, ComponentStartup>(OnTendrilStartup);
        SubscribeLocalEvent<TendrilMobComponent, MobStateChangedEvent>(OnMobState);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<TendrilComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.Mobs.Count >= comp.MaxSpawns)
                continue;
            if (comp.LastSpawn + TimeSpan.FromSeconds(comp.SpawnDelay) > _time.CurTime)
                continue;

            var xform = Transform(uid);
            var coords = xform.Coordinates;
            var newCoords = coords.Offset(_random.NextVector2(4));
            for (var i = 0; i < 20; i++)
            {
                var randVector = _random.NextVector2(4);
                newCoords = coords.Offset(randVector);
                if (!_lookup.GetEntitiesIntersecting(newCoords.ToMap(EntityManager, _transform), LookupFlags.Static).Any())
                {
                    break;
                }
            }
            var mob = Spawn(_random.Pick(comp.Spawns), newCoords);
            var mobComp = EnsureComp<TendrilMobComponent>(mob);
            mobComp.Tendril = uid;
            comp.Mobs.Add(mob);
            comp.LastSpawn = _time.CurTime;
        }
    }

    private void OnTendrilStartup(EntityUid uid, TendrilComponent comp, ComponentStartup args)
    {
        comp.LastSpawn = _time.CurTime;
    }

    private void OnTendrilMobDeath(EntityUid uid, TendrilComponent comp, ref TendrilMobDeadEvent args)
    {
        comp.Mobs.Remove(args.Entity);
    }

    private void OnTendrilDestruction(EntityUid uid, TendrilComponent comp, DestructionEventArgs args)
    {
        var coords = Transform(uid).Coordinates;
        Timer.Spawn(TimeSpan.FromSeconds(comp.ChasmDelay),
            () =>
        {
            SpawnChasm(coords, comp.ChasmRadius);
        });
    }

    private void SpawnChasm(EntityCoordinates coords, int radius)
    {
        Spawn("FloorChasmEntity", coords);
        for (var i = 1; i <= radius; i++)
        {
            // shitcode
            Spawn("FloorChasmEntity", new EntityCoordinates(coords.EntityId, coords.X + i, coords.Y));
            Spawn("FloorChasmEntity", new EntityCoordinates(coords.EntityId, coords.X - i, coords.Y));
            Spawn("FloorChasmEntity", new EntityCoordinates(coords.EntityId, coords.X, coords.Y + i));
            Spawn("FloorChasmEntity", new EntityCoordinates(coords.EntityId, coords.X, coords.Y - i));
            Spawn("FloorChasmEntity", new EntityCoordinates(coords.EntityId, coords.X + i, coords.Y + i));
            Spawn("FloorChasmEntity", new EntityCoordinates(coords.EntityId, coords.X - i, coords.Y + i));
            Spawn("FloorChasmEntity", new EntityCoordinates(coords.EntityId, coords.X + i, coords.Y - i));
            Spawn("FloorChasmEntity", new EntityCoordinates(coords.EntityId, coords.X - i, coords.Y - i));
        }

    }
    private void OnMobState(EntityUid uid, TendrilMobComponent comp, MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;
        if (!comp.Tendril.HasValue)
            return;
        var ev = new TendrilMobDeadEvent(uid);
        RaiseLocalEvent(comp.Tendril.Value, ref ev);
    }
}
