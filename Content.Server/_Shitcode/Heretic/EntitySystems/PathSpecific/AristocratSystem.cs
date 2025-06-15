// SPDX-FileCopyrightText: 2024 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Marcus F <199992874+thebiggestbruh@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 the biggest bruh <199992874+thebiggestbruh@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Goobstation.Heretic.EntitySystems.PathSpecific;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Components;
using Content.Server.Audio;
using Content.Server.Ghost;
using Content.Server.Light.Components;
using Content.Server.Light.EntitySystems;
using Content.Server.Heretic.Components.PathSpecific;
using Content.Shared.Atmos;
using Content.Shared.Audio;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.Damage;
using Content.Shared.Heretic;
using Content.Shared.Maps;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Content.Shared.Weather;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Player;
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
    [Dependency] private readonly IMapManager _mapMan = default!;
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly GhostSystem _ghost = default!;
    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly VoidCurseSystem _voidcurse = default!;
    [Dependency] private readonly ServerGlobalSoundSystem _globalSound = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly PoweredLightSystem _light = default!;
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly SharedWeatherSystem _weather = default!;
    [Dependency] private readonly TagSystem _tag = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AristocratComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<AristocratComponent, MobStateChangedEvent>(OnMobStateChange);
    }

    private void OnStartup(Entity<AristocratComponent> ent, ref ComponentStartup args)
    {
        BeginWaltz(ent);
        DoVoidAnnounce(ent, "begin");
    }

    private bool CheckOtherAristocrats(Entity<AristocratComponent> ent)
    {
        var others = EntityQueryEnumerator<AristocratComponent, MobStateComponent>();
        while (others.MoveNext(out var other, out _, out var stateComp))
        {
            if (ent.Owner == other
                || stateComp.CurrentState == MobState.Dead)
                continue;

            return true;
        }
        return false;
    }

    private void DoVoidAnnounce(Entity<AristocratComponent> ent, string context)
    {
        if (CheckOtherAristocrats(ent))
            return;

        var xform = Transform(ent);

        var victims = EntityQueryEnumerator<ActorComponent, MobStateComponent>();
        while (victims.MoveNext(out var victim, out var actorComp, out var stateComp))
        {
            var xformVictim = Transform(victim);

            if (xformVictim.MapUid != xform.MapUid
                || stateComp.CurrentState == MobState.Dead
                || ent.Owner == victim) // DoVoidAnnounce doesn't happen when there's other (alive) ascended void heretics, so you only have to exclude the user
                continue;

            _popup.PopupEntity(Loc.GetString($"void-ascend-{context}"), victim, actorComp.PlayerSession, PopupType.LargeCaution);
        }
    }

    private void BeginWaltz(Entity<AristocratComponent> ent)
    {
        if (CheckOtherAristocrats(ent))
            return;

        _globalSound.DispatchStationEventMusic(ent, ent.Comp.VoidsEmbrace, StationEventMusicType.VoidAscended, AudioParams.Default.WithLoop(true));

        // the fog (snow) is coming
        var xform = Transform(ent);
        _weather.SetWeather(xform.MapID, _prot.Index<WeatherPrototype>("SnowfallMagic"), null);
    }

    private void EndWaltz(Entity<AristocratComponent> ent)
    {
        if (CheckOtherAristocrats(ent))
            return;

        _globalSound.StopStationEventMusic(ent, StationEventMusicType.VoidAscended);

        var xform = Transform(ent);
        _weather.SetWeather(xform.MapID, null, null);
    }

    private void OnMobStateChange(Entity<AristocratComponent> ent, ref MobStateChangedEvent args)
    {
        var stateComp = args.Component;

        if (stateComp.CurrentState == MobState.Dead)
        {
            ent.Comp.HasDied = true;
            EndWaltz(ent); // its over bros
            DoVoidAnnounce(ent, "end");
        }

        if (stateComp.CurrentState == MobState.Alive
            && ent.Comp.HasDied) // in the rare case that they are revived for whatever reason
        {
            ent.Comp.HasDied = false;
            BeginWaltz(ent); // we're back bros
            DoVoidAnnounce(ent, "restart");
        }
    }

    private List<TileRef>? GetTiles(Entity<AristocratComponent> ent)
    {
        var xform = Transform(ent);

        var range = (int) ent.Comp.Range;

        var tilerefs = new List<TileRef>();

        var tileSelects = (range * range) * 2; // roughly 1/2 of the area's tiles should get selected per cycle
        for (int i = 0; i < tileSelects; i++)
        {
            var xOffset = _rand.Next(-range, range);
            var yOffset = _rand.Next(-range, range);
            var offsetValue = new Vector2(xOffset, yOffset);

            var coords = xform.Coordinates.Offset(offsetValue).SnapToGrid(EntityManager, _mapMan);
            var tile = coords.GetTileRef(EntityManager, _mapMan);

            if (tile.HasValue)
                tilerefs.Add(tile.Value);
        }

        return tilerefs;
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
        if (ent.Comp.HasDied) // powers will only take effect for as long as we're alive
            return;

        var coords = Transform(ent).Coordinates;
        var step = ent.Comp.UpdateStep;

        switch (step)
        {
            case 0:
                ExtinguishFires(ent, coords);
                break;
            case 1:
                FreezeAtmos(ent);
                break;
            case 2:
                DoChristmas(ent, coords);
                break;
            case 3:
                SpookyLights(ent, coords);
                break;
            case 4:
                FreezeNoobs(ent, coords);
                break;
            default:
                ent.Comp.UpdateStep = 0;
                break;
        }

        ent.Comp.UpdateStep++;
    }

    // makes shit cold
    private void FreezeAtmos(Entity<AristocratComponent> ent)
    {
        var mix = _atmos.GetTileMixture((ent, Transform(ent)));
        var freezingTemp = Atmospherics.T0C;

        if (mix != null)
        {
            if (mix.Temperature > freezingTemp)
                mix.Temperature = freezingTemp;

            mix.Temperature -= 100f;
        }
    }

    // extinguish gases on tiles
    private void ExtinguishFiresTiles(Entity<AristocratComponent> ent)
    {
        var tilerefs = GetTiles(ent);

        if (tilerefs == null
            || tilerefs.Count == 0)
            return;

        foreach (var tile in tilerefs)
            _atmos.HotspotExtinguish(tile.GridUid, tile.GridIndices);
    }

    // extinguish ppl and stuff
    private void ExtinguishFires(Entity<AristocratComponent> ent, EntityCoordinates coords)
    {
        var fires = _lookup.GetEntitiesInRange<FlammableComponent>(coords, ent.Comp.Range);

        foreach (var entity in fires)
        {
            if (entity.Comp.OnFire)
                _flammable.Extinguish(entity);
        }

        ExtinguishFiresTiles(ent);
    }

    // replaces certain things with their winter analogue (amongst other things)
    private void DoChristmas(Entity<AristocratComponent> ent, EntityCoordinates coords)
    {
        SpawnTiles(ent);

        var dspec = new DamageSpecifier();
        dspec.DamageDict.Add("Structural", 100);

        var tags = _lookup.GetEntitiesInRange<TagComponent>(coords, ent.Comp.Range);

        foreach (var tag in tags)
        {
            // walls
            if (_tag.HasTag(tag.Owner, "Wall")
                && _rand.Prob(.45f)
                && Prototype(tag) != null
                && Prototype(tag)!.ID != SnowWallPrototype)
            {
                Spawn(SnowWallPrototype, Transform(tag).Coordinates);
                QueueDel(tag);
            }

            // windows
            if (_tag.HasTag(tag.Owner, "Window")
                && Prototype(tag) != null)
                _damage.TryChangeDamage(tag, dspec, origin: ent);
        }
    }

    // kill the lights
    private void SpookyLights(Entity<AristocratComponent> ent, EntityCoordinates coords)
    {
        var lights = _lookup.GetEntitiesInRange<PoweredLightComponent>(coords, ent.Comp.Range);

        foreach (var light in lights)
            _light.TryDestroyBulb(light);
    }

    // curses noobs
    private void FreezeNoobs(Entity<AristocratComponent> ent, EntityCoordinates coords)
    {
        var noobs = _lookup.GetEntitiesInRange<MobStateComponent>(coords, ent.Comp.Range);

        foreach (var noob in noobs)
        {
            // ignore same path heretics and ghouls
            if (HasComp<HereticComponent>(noob)
                || HasComp<GhoulComponent>(noob))
                continue;

            _voidcurse.DoCurse(noob);
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

        var tilerefs = GetTiles(ent);

        if (tilerefs == null
            || tilerefs.Count == 0)
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
