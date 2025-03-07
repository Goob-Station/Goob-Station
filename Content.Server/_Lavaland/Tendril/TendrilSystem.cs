using Content.Server._Lavaland.Tendril.Components;
using Content.Shared.Damage;
using Content.Shared.Destructible;
using Content.Shared.FixedPoint;
using Content.Shared.Mobs;
using Content.Shared.Popups;
using Robust.Shared.Map;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Lavaland.Tendril;

public sealed class TendrilSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
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
            comp.UpdateAccumulator += frameTime;

            if (comp.UpdateAccumulator < comp.UpdateFrequency)
                continue;

            comp.UpdateAccumulator = 0;

            if (comp.Mobs.Count >= comp.MaxSpawns)
                continue;

            if (comp.LastSpawn + TimeSpan.FromSeconds(comp.SpawnDelay) > _time.CurTime)
                continue;

            var mob = Spawn(_random.Pick(comp.Spawns), Transform(uid).Coordinates);
            var mobComp = EnsureComp<TendrilMobComponent>(mob);
            mobComp.Tendril = uid;
            comp.Mobs.Add(mob);
            comp.LastSpawn = _time.CurTime;
        }
    }

    private void OnTendrilStartup(EntityUid uid, TendrilComponent comp, ComponentStartup args)
    {
        comp.LastSpawn = _time.CurTime + TimeSpan.FromSeconds(5);
    }

    private void OnTendrilMobDeath(EntityUid uid, TendrilComponent comp, ref TendrilMobDeadEvent args)
    {
        comp.Mobs.Remove(args.Entity);
        comp.DefeatedMobs++;

        // John Shitcode
        if (comp.DefeatedMobs >= comp.MobsToDefeat)
        {
            comp.DestroyedWithMobs = true;
            _damage.TryChangeDamage(uid, new DamageSpecifier { DamageDict = new Dictionary<string, FixedPoint2> {{ "Blunt", 1000 }} });
        }
    }

    private void OnTendrilDestruction(EntityUid uid, TendrilComponent comp, DestructionEventArgs args)
    {
        var coords = Transform(uid).Coordinates;
        var delay = comp.ChasmDelay;

        if (comp.DestroyedWithMobs)
            delay = comp.ChasmDelayOnMobsDefeat;

        _popup.PopupCoordinates(Loc.GetString("tendril-destroyed-warning-message"), coords, PopupType.LargeCaution);

        Timer.Spawn(TimeSpan.FromSeconds(delay),
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
