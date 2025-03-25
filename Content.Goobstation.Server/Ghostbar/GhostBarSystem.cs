using Content.Goobstation.Server.Ghostbar.Components;
using Content.Server.Antag.Components;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Events;
using Content.Server.Mind;
using Content.Server.Station.Systems;
using Content.Shared.Ghost;
using Content.Shared.Mind.Components;
using Content.Shared.Mindshield.Components;
using Content.Shared.Players;
using Content.Shared.Roles;
using Robust.Shared.EntitySerialization;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Ghostbar;

public sealed class GhostBarSystem : EntitySystem
{
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly GameTicker _ticker = default!;
    [Dependency] private readonly StationSpawningSystem _spawningSystem = default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;

    private static readonly List<ProtoId<JobPrototype>> _jobComponents = new()
    {
        "Passenger", "Bartender", "Botanist", "Chef", "Janitor"
    };

    public override void Initialize()
    {
        SubscribeLocalEvent<RoundStartingEvent>(OnRoundStart);
        SubscribeNetworkEvent<GhostBarSpawnEvent>(SpawnPlayer);
        SubscribeLocalEvent<GhostBarPlayerComponent, MindRemovedMessage>(OnPlayerGhosted);
    }

    const string MapPath = "Maps/_Goobstation/Nonstations/ghostbar.yml";
    private void OnRoundStart(RoundStartingEvent ev)
    {
        var resPath = new ResPath(MapPath);

        if (_mapLoader.TryLoadMap(resPath, out var map, out _, new DeserializationOptions { InitializeMaps = true }))
            _mapSystem.SetPaused(map.Value.Comp.MapId, false);
    }

    public void SpawnPlayer(GhostBarSpawnEvent msg, EntitySessionEventArgs args)
    {
        var player = args.SenderSession;

        if (!_mindSystem.TryGetMind(player, out var mindId, out var mind))
        {
            Log.Warning($"Failed to find mind for player {player.Name}.");
            return;
        }

        if (!_entityManager.TryGetComponent<GhostComponent>(player.AttachedEntity, out var ghost))
        {
            Log.Warning($"User {player.Name} tried to spawn at ghost bar without being a ghost.");
            return;
        }

        if (!ghost.CanEnterGhostBar)
        {
            Log.Warning($"User {player.Name} tried to enter ghost bar while they cannot enter it.");
            return;
        }

        var spawnPoints = new List<EntityCoordinates>();
        var query = EntityQueryEnumerator<GhostBarSpawnComponent>();
        while (query.MoveNext(out var ent, out _))
        {
            spawnPoints.Add(_entityManager.GetComponent<TransformComponent>(ent).Coordinates);
        }

        if (spawnPoints.Count == 0)
        {
            Log.Warning("No spawn points found for ghost bar.");
            return;
        }

        var data = player.ContentData();

        if (data == null)
        {
            Log.Warning($"ContentData was null when trying to spawn {player.Name} in ghost bar.");
            return;
        }

        var randomSpawnPoint = _random.Pick(spawnPoints);
        var randomJob = _random.Pick(_jobComponents);
        var profile = _ticker.GetPlayerProfile(args.SenderSession);
        var mobUid = _spawningSystem.SpawnPlayerMob(randomSpawnPoint, randomJob, profile, null);

        _entityManager.EnsureComponent<GhostBarPlayerComponent>(mobUid);
        _entityManager.EnsureComponent<MindShieldComponent>(mobUid);
        _entityManager.EnsureComponent<AntagImmuneComponent>(mobUid);
        _entityManager.EnsureComponent<IsDeadICComponent>(mobUid);

        if (mind.Objectives.Count == 0)
            _mindSystem.WipeMind(player);
        mindId = _mindSystem.CreateMind(data.UserId, profile.Name).Owner;
        _mindSystem.TransferTo(mindId, mobUid, true);
    }

    private void OnPlayerGhosted(EntityUid uid, GhostBarPlayerComponent component, MindRemovedMessage args)
    {
        _entityManager.DeleteEntity(uid);
    }
}

