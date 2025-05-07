// SPDX-FileCopyrightText: 2025 AftrLite <61218133+AftrLite@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Server.Actions;
using Content.Server.Popups;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared._DV.CosmicCult;
using Content.Shared.Maps;
using Robust.Shared.Map.Components;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.CosmicCult.Abilities;

public sealed class CosmicMonumentSystem : EntitySystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly CosmicCultRuleSystem _cultRule = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDef = default!;
    [Dependency] private readonly MonumentSystem _monument = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly StationSystem _station = default!;

    private static readonly EntProtoId MonumentCollider = "MonumentCollider";
    private static readonly EntProtoId MonumentCosmicCultMoveEnd = "MonumentCosmicCultMoveEnd";
    private static readonly EntProtoId MonumentCosmicCultMoveStart = "MonumentCosmicCultMoveStart";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CosmicCultLeadComponent, EventCosmicPlaceMonument>(OnCosmicPlaceMonument);
        SubscribeLocalEvent<CosmicCultLeadComponent, EventCosmicMoveMonument>(OnCosmicMoveMonument);
    }

    //todo attack this with a debugger at some point, it seems to un-prime before it should sometimes?
    //no idea why, might be something to do with verifying placement inside the action's execution instead of in an attemptEvent beforehand?
    //yeah it is - if the action is primed but fails at this step, then the action becomes un-primed but does not properly go through, requiring it to be primed again
    //works fine:tm: for now with a slightly jank fix on the client end of things, will probably want to dig deeper?
    //actually might not want to fix it?
    //I've got the client stuff working well & this works out to making the ghost stay up so long as you consistently try (& fail) to place the monument
    //guess I should ask for specific feedback for this one tiny feature?
    private void OnCosmicPlaceMonument(Entity<CosmicCultLeadComponent> uid, ref EventCosmicPlaceMonument args)
    {
        if (!VerifyPlacement(uid, out var pos))
            return;

        _actions.RemoveAction(uid, uid.Comp.CosmicMonumentPlaceActionEntity);

        Spawn(MonumentCollider, pos);
        var monument = Spawn(uid.Comp.MonumentPrototype, pos);

        _cultRule.TransferCultAssociation(uid, monument);
    }

    private void OnCosmicMoveMonument(Entity<CosmicCultLeadComponent> uid, ref EventCosmicMoveMonument args)
    {
        if (_cultRule.AssociatedGamerule(uid) is not { } cult
            || !VerifyPlacement(uid, out var pos))
            return;

        _actions.RemoveAction(uid, uid.Comp.CosmicMonumentMoveActionEntity);

        //delete all old monument colliders for 100% safety
        var colliderQuery = EntityQueryEnumerator<MonumentCollisionComponent>();

        while (colliderQuery.MoveNext(out var collider, out _))
            QueueDel(collider);

        //spawn the destination effect first because we only need one
        var destEnt = Spawn(MonumentCosmicCultMoveEnd, pos);
        var destComp = EnsureComp<MonumentMoveDestinationComponent>(destEnt);
        destComp.Monument = cult.Comp.MonumentInGame;
        var coords = Transform(cult.Comp.MonumentInGame).Coordinates;
        Spawn(MonumentCollider, pos); //spawn a new collider
        Spawn(MonumentCosmicCultMoveStart, coords);
        Spawn(MonumentCollider, Transform(cult.Comp.MonumentInGame).Coordinates); //spawn a new collider
        _monument.PhaseOutMonument(cult.Comp.MonumentInGame);
        destComp.PhaseInTimer = cult.Comp.MonumentInGame.Comp.PhaseOutTimer + TimeSpan.FromSeconds(0.75);
    }

    //todo this can probably be mostly moved to shared but my brain isn't cooperating w/ that rn
    private bool VerifyPlacement(Entity<CosmicCultLeadComponent> uid, out EntityCoordinates outPos)
    {
        //MAKE SURE WE'RE STANDING ON A GRID
        var xform = Transform(uid);
        outPos = new EntityCoordinates();

        if (!TryComp<MapGridComponent>(xform.GridUid, out var grid))
        {
            _popup.PopupEntity(Loc.GetString("cosmicability-monument-spawn-error-grid"), uid, uid);
            return false;
        }

        var localTile = _map.GetTileRef(xform.GridUid.Value, grid, xform.Coordinates);
        var targetIndices = localTile.GridIndices + new Vector2i(0, 1);
        var pos = _map.ToCenterCoordinates(xform.GridUid.Value, targetIndices, grid);
        outPos = pos;
        var box = new Box2(pos.Position + new Vector2(-1.4f, -0.4f), pos.Position + new Vector2(1.4f, 0.4f));

        //CHECK IF IT'S BEING PLACED CHEESILY CLOSE TO SPACE
        var spaceDistance = 3;
        var worldPos = _transform.GetWorldPosition(xform);
        foreach (var tile in _map.GetTilesIntersecting(xform.GridUid.Value, grid, new Circle(worldPos, spaceDistance)))
        {
            if (tile.IsSpace(_tileDef))
            {
                _popup.PopupEntity(Loc.GetString("cosmicability-monument-spawn-error-space", ("DISTANCE", spaceDistance)), uid, uid);
                return false;
            }
        }

        //CHECK IF WE'RE ON THE STATION OR IF SOMEONE'S TRYING TO SNEAK THIS ONTO SOMETHING SMOL
        var station = _station.GetStationInMap(xform.MapID);

        EntityUid? stationGrid = null;

        if (TryComp<StationDataComponent>(station, out var stationData))
            stationGrid = _station.GetLargestGrid(stationData);

        if (stationGrid is not null && stationGrid != xform.GridUid)
        {
            _popup.PopupEntity(Loc.GetString("cosmicability-monument-spawn-error-station"), uid, uid);
            return false;
        }

        //CHECK FOR ENTITY AND ENVIRONMENTAL INTERSECTIONS
        if (_lookup.AnyLocalEntitiesIntersecting(xform.GridUid.Value, box, LookupFlags.Dynamic | LookupFlags.Static, uid))
        {
            _popup.PopupEntity(Loc.GetString("cosmicability-monument-spawn-error-intersection"), uid, uid);
            return false;
        }

        return true;
    }
}
