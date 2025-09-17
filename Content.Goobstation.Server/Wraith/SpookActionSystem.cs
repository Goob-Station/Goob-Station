using System.Linq;
using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Goobstation.Shared.Wraith.Spook;
using Content.Server.Doors.Systems;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Fluids.EntitySystems;
using Content.Server.Ghost;
using Content.Server.Light.Components;
using Content.Server.Light.EntitySystems;
using Content.Server.Respawn;
using Content.Server.Storage.Components;
using Content.Server.Storage.EntitySystems;
using Content.Shared.Containers;
using Content.Shared.Doors.Components;
using Robust.Server.Containers;
using Robust.Server.GameObjects;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Wraith;

public sealed class SpookActionSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly PoweredLightSystem _poweredLight = default!;
    [Dependency] private readonly ExplosionSystem _explosion = default!;
    [Dependency] private readonly DoorSystem _door = default!;
    [Dependency] private readonly EntityStorageSystem _entityStorage = default!;
    [Dependency] private readonly SmokeSystem _smoke = default!;
    [Dependency] private readonly SpecialRespawnSystem _respawn = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    private EntityQuery<PoweredLightComponent> _poweredLightQuery;
    private EntityQuery<DoorComponent> _doorQuery;
    private EntityQuery<EntityStorageComponent> _entityStorageQuery;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        _poweredLightQuery = GetEntityQuery<PoweredLightComponent>();
        _doorQuery = GetEntityQuery<DoorComponent>();
        _entityStorageQuery = GetEntityQuery<EntityStorageComponent>();

        SubscribeLocalEvent<FlipLightsComponent, FlipLightsEvent>(OnFlipLights);
        SubscribeLocalEvent<BurnLightsComponent, BurnLightsEvent>(OnBurnLights);
        SubscribeLocalEvent<OpenDoorsSpookComponent, OpenDoorsSpookEvent>(OnOpenDoors);
        SubscribeLocalEvent<CreateSpookSmokeComponent, CreateSmokeSpookEvent>(OnCreateSmoke);
        SubscribeLocalEvent<CreateEctoplasmComponent, CreateEctoplasmEvent>(OnCreateEctoplasm);
    }
    // todo: playtest
    // todo: Sap APC, Haunt PDAs (probably not), Random

    private void OnFlipLights(Entity<FlipLightsComponent> ent, ref FlipLightsEvent args)
    {
        // taken from ghost boo system

        if (args.Handled)
            return;

        var entities = _lookup.GetEntitiesInRange(args.Performer, ent.Comp.FlipLightRadius).ToList();
        _random.Shuffle(entities);

        var booCounter = 0;
        foreach (var entity in entities)
        {
            var ev = new GhostBooEvent();
            RaiseLocalEvent(entity, ev);

            if (ev.Handled)
                booCounter++;

            if (booCounter >= ent.Comp.FlipLightMaxTargets)
                break;
        }

        args.Handled = true;
    }

    private void OnBurnLights(Entity<BurnLightsComponent> ent, ref BurnLightsEvent args)
    {
        var entities = _lookup.GetEntitiesInRange(args.Performer, ent.Comp.SearchRadius).ToList();

        var lightBrokenCounter = 0;
        foreach (var entity in entities)
        {
            if (lightBrokenCounter > ent.Comp.MaxBurnLights)
                break;

            if (!_poweredLightQuery.TryComp(entity, out var poweredLight))
                continue;

            _poweredLight.TryDestroyBulb(entity, poweredLight);

            var explosion = Spawn("PipeBomb");
            _explosion.TriggerExplosive(explosion);

            lightBrokenCounter++;
        }

        args.Handled = true;
    }

    private void OnOpenDoors(Entity<OpenDoorsSpookComponent> ent, ref OpenDoorsSpookEvent args)
    {
        var entities = _lookup.GetEntitiesInRange(ent.Owner, ent.Comp.SearchRadius).ToList();
        _random.Shuffle(entities);

        var openedCounter = 0;
        foreach (var entity in entities)
        {
            if (openedCounter > ent.Comp.MaxContainer)
                break;

            if (_entityStorageQuery.HasComp(entity))
            {
                _entityStorage.TryOpenStorage(ent.Owner, entity);
                openedCounter++;
                continue;
            }

            if (_doorQuery.HasComp(entity))
            {
                _door.TryOpen(entity);
                openedCounter++;
            }
        }
    }

    private void OnCreateSmoke(Entity<CreateSpookSmokeComponent> ent, ref CreateSmokeSpookEvent args)
    {
        // TODO make reagent that makes you drop items in smoke
        var grid = _transform.GetGrid(ent.Owner);
        var map = _transform.GetMap(ent.Owner);

        if (map == null || grid == null)
            return;

        for (var i = 0; i < ent.Comp.SmokeAmount; i++)
        {
            if (!_respawn.TryFindRandomTile(grid.Value, map.Value, 10, out var coords, false))
                continue;

            var smokeEnt = SpawnAtPosition(ent.Comp.SmokeProto, coords);
            _smoke.StartSmoke(smokeEnt, ent.Comp.SmokeSolution, ent.Comp.Duration, ent.Comp.SpreadAmount);
        }
    }

    private void OnCreateEctoplasm(Entity<CreateEctoplasmComponent> ent, ref CreateEctoplasmEvent args)
    {
        var grid = _transform.GetGrid(ent.Owner);
        var map = _transform.GetMap(ent.Owner);

        if (map == null || grid == null)
            return;

        var amount = _random.Next(ent.Comp.AmountMinMax.X, ent.Comp.AmountMinMax.Y + 1);
        for (var i = 0; i < amount; i++)
        {
            if (!_respawn.TryFindRandomTile(grid.Value, map.Value, 6, out var coords, false))
                continue;

            SpawnAtPosition(ent.Comp.EctoplasmProto, coords);
        }
    }
}
