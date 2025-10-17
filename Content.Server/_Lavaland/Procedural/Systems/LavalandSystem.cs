// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aineias1 <dmitri.s.kiselev@gmail.com>
// SPDX-FileCopyrightText: 2025 FaDeOkno <143940725+FaDeOkno@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 McBosserson <148172569+McBosserson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Milon <plmilonpl@gmail.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
// SPDX-FileCopyrightText: 2025 SX_7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Unlumination <144041835+Unlumy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Lavaland.Procedural.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Decals;
using Content.Server.Parallax;
using Content.Server.Shuttles.Systems;
using Content.Shared._Lavaland.Procedural.Prototypes;
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

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
        SubscribeLocalEvent<MobStateComponent, EntParentChangedMessage>(OnPlayerParentChange);

        _gridQuery = GetEntityQuery<MapGridComponent>();
        _xformQuery = GetEntityQuery<TransformComponent>();
        _fixtureQuery = GetEntityQuery<FixturesComponent>();

        Subs.CVar(_config, CCVars.LavalandEnabled, value => LavalandEnabled = value, true);
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

    /// <summary>
    /// Setup ALL instances of LavalandMapPrototype.
    /// </summary>
    public void SetupLavalands()
    {
        foreach (var lavaland in _proto.EnumeratePrototypes<LavalandMapPrototype>())
        {
            if (!SetupLavalandPlanet(out _, lavaland))
            {
                Log.Error($"Failed to load lavaland planet: {lavaland.ID}");
            }
        }
    }
}
