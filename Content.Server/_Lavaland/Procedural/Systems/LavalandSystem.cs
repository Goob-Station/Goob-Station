// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Lavaland.Procedural.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Decals;
using Content.Server.GameTicking;
using Content.Server.Parallax;
using Content.Server.Procedural;
using Content.Server.Shuttles.Systems;
using Content.Shared.CCVar;
using Content.Shared.GameTicking;
using Content.Shared.Mobs.Components;
using Content.Shared.Maps;
using Robust.Shared.Configuration;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

// ReSharper disable EnforceForeachStatementBraces
namespace Content.Server._Lavaland.Procedural.Systems;

public sealed partial class LavalandSystem : EntitySystem
{
    public bool LavalandEnabled = true;

    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly TileSystem _tile = default!;
    [Dependency] private readonly ITileDefinitionManager _tiledef = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly INetConfigurationManager _config = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly BiomeSystem _biome = default!;
    [Dependency] private readonly DecalSystem _decals = default!;
    [Dependency] private readonly DungeonSystem _dungeon = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly ShuttleSystem _shuttle = default!;

    private EntityQuery<MapGridComponent> _gridQuery;
    private EntityQuery<TransformComponent> _xformQuery;
    private EntityQuery<FixturesComponent> _fixtureQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LoadingMapsEvent>(OnLoadingMaps);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
        SubscribeLocalEvent<MobStateComponent, EntParentChangedMessage>(OnPlayerParentChange);

        _gridQuery = GetEntityQuery<MapGridComponent>();
        _xformQuery = GetEntityQuery<TransformComponent>();
        _fixtureQuery = GetEntityQuery<FixturesComponent>();

        Subs.CVar(_config, CCVars.LavalandEnabled, value => LavalandEnabled = value, true);
    }

    private void OnLoadingMaps(LoadingMapsEvent ev)
    {
        EnsurePreloaderMap();
        foreach (var gameMap in ev.Maps)
        {
            foreach (var planetEntry in gameMap.Planets)
            {
                SetupLavalandPlanet(planetEntry, out _);
            }
        }
    }

    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        var ent = GetPreloaderEntity();
        if (ent == null)
            return;

        Del(ent.Value.Owner);
    }

    public void EnsurePreloaderMap()
    {
        // Already have a preloader?
        if (GetPreloaderEntity() != null
            || !LavalandEnabled)
            return;

        var mapUid = _map.CreateMap(out var mapId, false);
        EnsureComp<LavalandPreloaderComponent>(mapUid);
        _metaData.SetEntityName(mapUid, "Lavaland Preloader Map");
        _map.SetPaused(mapId, true);
    }

    /// <summary>
    /// Raised when an entity exits or enters a grid.
    /// </summary>
    private void OnPlayerParentChange(Entity<MobStateComponent> ent, ref EntParentChangedMessage args)
    {
        if (TerminatingOrDeleted(ent.Owner))
            return;

        if (args.OldParent != null
            && TryComp<LavalandGridGrantComponent>(args.OldParent.Value, out var toRemove))
            EntityManager.RemoveComponents(ent.Owner, toRemove.ComponentsToGrant);
        else if (TryComp<LavalandGridGrantComponent>(Transform(ent.Owner).GridUid, out var toGrant))
            EntityManager.AddComponents(ent.Owner, toGrant.ComponentsToGrant);
    }

    public Entity<LavalandPreloaderComponent>? GetPreloaderEntity()
    {
        var query = AllEntityQuery<LavalandPreloaderComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            return (uid, comp);
        }

        return null;
    }

    public List<Entity<LavalandMapComponent>> GetLavalands()
    {
        var lavalandsQuery = EntityQueryEnumerator<LavalandMapComponent>();
        var lavalands = new List<Entity<LavalandMapComponent>>();
        while (lavalandsQuery.MoveNext(out var uid, out var comp))
        {
            lavalands.Add((uid, comp));
        }

        return lavalands;
    }
}
