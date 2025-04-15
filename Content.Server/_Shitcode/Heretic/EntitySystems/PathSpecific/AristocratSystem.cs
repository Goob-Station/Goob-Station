// SPDX-FileCopyrightText: 2024 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Goobstation.Heretic.EntitySystems.PathSpecific;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Audio;
using Content.Server.Heretic.Components.PathSpecific;
using Robust.Shared.Audio;
using Content.Shared.Heretic;
using Content.Shared.Maps;
using Content.Shared.Tag;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using System.Linq;
using System.Numerics;

namespace Content.Server.Heretic.EntitySystems.PathSpecific;

// void path heretic exclusive
public sealed partial class AristocratSystem : EntitySystem
{
    [Dependency] private readonly TileSystem _tile = default!;
    [Dependency] private readonly IRobustRandom _rand = default!;
    [Dependency] private readonly IPrototypeManager _prot = default!;
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly VoidCurseSystem _voidcurse = default!;
    [Dependency] private readonly ServerGlobalSoundSystem _globalSound = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AristocratComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(Entity<AristocratComponent> ent, ref ComponentStartup args)
    {
        // mmm original soundtractk
        _globalSound.PlayGlobalOnStation(ent, "/Audio/_Goobstation/Heretic/Ambience/Antag/Heretic/VoidsEmbrace.ogg", AudioParams.Default);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<AristocratComponent>();
        while (query.MoveNext(out var uid, out var aristocrat))
        {
            if (!uid.IsValid())
                continue;

            aristocrat.UpdateTimer += frameTime;

            if (aristocrat.UpdateTimer >= aristocrat.UpdateDelay)
            {
                Cycle((uid, aristocrat));
                aristocrat.UpdateTimer = 0;
            }
        }
    }

    private void Cycle(Entity<AristocratComponent> ent)
    {
        var lookup = _lookup.GetEntitiesInRange(Transform(ent).Coordinates, ent.Comp.Range);

        FreezeAtmos(ent);

        DoChristmas(ent, lookup);

        FreezeNoobs(ent, lookup);
    }

    // makes shit cold
    private void FreezeAtmos(Entity<AristocratComponent> ent)
    {
        var mix = _atmos.GetTileMixture((ent, Transform(ent)));
        if (mix != null)
            mix.Temperature -= 50f;
    }

    // replaces certain things with their winter analogue
    private void DoChristmas(Entity<AristocratComponent> ent, HashSet<EntityUid> lookup)
    {
        SpawnTiles(ent);

        foreach (var look in lookup)
        {
            if (TryComp<TagComponent>(look, out var tag))
            {
                var tags = tag.Tags;

                // walls
                if (_rand.Prob(.45f) && tags.Contains("Wall")
                && Prototype(look) != null && Prototype(look)!.ID != SnowWallPrototype)
                {
                    Spawn(SnowWallPrototype, Transform(look).Coordinates);
                    QueueDel(look);
                }
            }
        }
    }

    // curses noobs
    private void FreezeNoobs(Entity<AristocratComponent> ent, HashSet<EntityUid> lookup)
    {
        foreach (var look in lookup)
        {
            // ignore same path heretics and ghouls
            if (HasComp<HereticComponent>(look) || HasComp<GhoulComponent>(look))
                continue;

            _voidcurse.DoCurse(look);
        }
    }

    private static readonly string SnowTilePrototype = "FloorAstroSnow";
    [ValidatePrototypeId<EntityPrototype>] private static readonly EntProtoId SnowWallPrototype = "WallSnowCobblebrick";
    [ValidatePrototypeId<EntityPrototype>] private static readonly EntProtoId BoobyTrapTile = "TileHereticVoid";

    private void SpawnTiles(Entity<AristocratComponent> ent)
    {
        var xform = Transform(ent);

        if (!TryComp<MapGridComponent>(xform.GridUid, out var grid))
            return;

        var pos = xform.Coordinates.Position;
        var box = new Box2(pos + new Vector2(-ent.Comp.Range, -ent.Comp.Range), pos + new Vector2(ent.Comp.Range, ent.Comp.Range));
        var tilerefs = _map.GetLocalTilesIntersecting((EntityUid) xform.GridUid, grid, box).ToList();

        if (tilerefs.Count == 0)
            return;

        var tiles = new List<TileRef>();
        var tiles2 = new List<TileRef>();
        foreach (var tile in tilerefs)
        {
            if (_rand.Prob(.45f))
                tiles.Add(tile);

            if (_rand.Prob(.25f))
                tiles2.Add(tile);
        }

        // it's christmas!!
        foreach (var tileref in tiles)
        {
            var tile = _prot.Index<ContentTileDefinition>(SnowTilePrototype);
            _tile.ReplaceTile(tileref, tile);
        }

        // boobytraps :trollface:
        foreach (var tileref in tiles2)
        {
            var tpos = _map.GridTileToWorld((EntityUid) xform.GridUid, grid, tileref.GridIndices);

            // this shit is for checking if there is a void trap already on that tile or not.
            var el = _lookup.GetEntitiesInRange(tpos, .25f).Where(e => Prototype(e)?.ID == BoobyTrapTile.Id).ToList();
            if (el.Count == 0)
                Spawn(BoobyTrapTile, tpos);
        }
    }
}