using Content.Goobstation.Server.Antag;
using Content.Goobstation.Server.Slasher.Components;
using Content.Server.Antag;
using Content.Server.Antag.Components;
using Content.Server.GameTicking.Rules;
using Content.Server.Station.Systems;
using Content.Server.Storage.EntitySystems;
using Content.Shared.GameTicking.Components;
using Content.Shared.Storage.Components;
using Content.Shared.Tag;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Slasher.Systems;

/// <summary>
/// Handles placing the antag ghost-role spawner inside a random station locker.
/// Works with AntagSelection.
/// </summary>
public sealed class AntagLockerSpawnSystem : GameRuleSystem<AntagLockerSpawnComponent>
{
    private static readonly ProtoId<TagPrototype> MaintenanceClosetTag = "MaintenanceCloset";

    [Dependency] private readonly AntagBetterRandomSpawnSystem _betterSpawn = default!;
    [Dependency] private readonly EntityStorageSystem _entityStorage = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly StationSystem _stationSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AntagLockerSpawnComponent, AntagSelectLocationEvent>(OnSelectLocation);
        SubscribeLocalEvent<AntagLockerSpawnComponent, AfterAntagEntitySelectedEvent>(OnAntagSelected);
    }

    private void OnSelectLocation(Entity<AntagLockerSpawnComponent> ent, ref AntagSelectLocationEvent args)
    {
        if (args.Session == null)
            return;

        if (ent.Comp.ChosenLocker is { } locker && Exists(locker))
            args.Coordinates.Add(_transform.GetMapCoordinates(locker));

        else if (ent.Comp.FallbackCoords is { } fallback)
            args.Coordinates.Add(_transform.ToMapCoordinates(fallback));
    }

    private void OnAntagSelected(Entity<AntagLockerSpawnComponent> ent, ref AfterAntagEntitySelectedEvent args)
    {
        if (args.Session == null)
            return;

        // Fallback mode: no locker was found, player is teleported via OnSelectLocation.
        if (ent.Comp.ChosenLocker is not { } locker || !Exists(locker))
            return;

        if (!TryComp<EntityStorageComponent>(locker, out var storage))
            return;

        _entityStorage.Insert(args.EntityUid, locker, storage);
    }

    protected override void ActiveTick(EntityUid uid, AntagLockerSpawnComponent comp, GameRuleComponent gameRule, float frameTime)
    {
        base.ActiveTick(uid, comp, gameRule, frameTime);

        if (comp.Placed)
            return;

        EntityUid? spawnerEnt = null;
        var spawnerQuery = EntityQueryEnumerator<GhostRoleAntagSpawnerComponent>();
        while (spawnerQuery.MoveNext(out var spawner, out var spawnerComp))
        {
            if (spawnerComp.Rule != uid)
                continue;

            spawnerEnt = spawner;
            break;
        }

        if (spawnerEnt == null
            || !TryGetRandomStation(out var station))
            return;

        var validLockers = new List<(EntityUid, EntityStorageComponent)>();
        var query = EntityQueryEnumerator<EntityStorageComponent, TransformComponent>();
        while (query.MoveNext(out var ent, out var storage, out var xform))
        {
            if (_stationSystem.GetOwningStation(ent, xform) != station
                || storage.Open
                || storage.Contents.ContainedEntities.Count >= storage.Capacity)
                continue;

            if (comp.MaintenanceOnly && !_tag.HasTag(ent, MaintenanceClosetTag))
                continue;

            validLockers.Add((ent, storage));
        }

        if (validLockers.Count == 0)
        {
            // No valid locker found; fall back to TryFindSafeRandomLocation.
            EntityCoordinates? safeCoords = null;
            if (_betterSpawn.TryFindSafeRandomLocation(out var betterCoords))
                safeCoords = betterCoords;

            if (safeCoords is { } coords)
            {
                _transform.SetMapCoordinates(spawnerEnt.Value, _transform.ToMapCoordinates(coords));
                comp.FallbackCoords = coords;
            }

            comp.Placed = true;
            return;
        }

        var (locker, storageComp) = RobustRandom.Pick(validLockers);
        if (!_entityStorage.Insert(spawnerEnt.Value, locker, storageComp))
            return;

        comp.ChosenLocker = locker;
        comp.Placed = true;
    }

}
