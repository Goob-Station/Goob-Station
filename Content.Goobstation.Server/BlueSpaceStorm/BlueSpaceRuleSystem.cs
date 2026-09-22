using Content.Server.Chat.Systems;
using Content.Shared.GameTicking.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;
using Robust.Shared.Prototypes;
using Content.Shared.Station.Components;
using Content.Server.Chemistry.Components;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Player;
using Robust.Shared.GameObjects;
using System;
using Content.Shared.Maps;
using Content.Shared.Physics;

namespace Content.Server.StationEvents.Events;

public sealed partial class BlueSpaceStormRuleSystem :
    StationEventSystem<BlueSpaceRuleComponent>
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly TurfSystem _turf = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private IMapManager _map = default!;
    [Dependency] private readonly SharedMapSystem _mapsys = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    protected override void Started(EntityUid uid, BlueSpaceRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        _audio.PlayGlobal(
            new SoundPathSpecifier("/Audio/Announcements/attention.ogg"),
            Filter.Broadcast(),
            true);

        _chat.DispatchGlobalAnnouncement(
            "Warning. Possible Bluespace phenomena detected, please alert the science team of any breaches in spacetime. Security team should prepare for potential threats..",
            colorOverride: Color.Cyan,
            playSound: false);

        if (!TryGetRandomStation(out var station))
        {
            ForceEndSelf(uid, gameRule);
            return;
        }

        if (!TryComp<StationDataComponent>(station.Value, out var stationData))
        {
            ForceEndSelf(uid, gameRule);
            return;
        }

        var gridUid = StationSystem.GetLargestGrid((station.Value, stationData));
        if (gridUid == null)
        {
            ForceEndSelf(uid, gameRule);
            return;
        }

        var portalPositions = new List<Vector2i>();

        //Portals will spawn if:
        //Non-spaced 3x3 area, with no impassable or high impassable tiles
        //No power infrastructure nearby (only checks for SMES and Substations)
        //No chemmasters nearby (Portals are destructive enough that would make chemistry useless for the rest of the shift)
        //Max portals: 4 with no dupes, can spawn less than 4 if no valid locations are found

        foreach (EntProtoId entityPrototype in component.AvailablePortals)
        {
            for (var i = 0; i < 100; i++)
            {
                if (!TryFindRandomTile(
                        out var tilePos,
                        out _,
                        out var grid,
                        out var coords))
                {
                    continue;
                }

                if (grid != gridUid.Value)
                    continue;

                if (_lookup.GetEntitiesInRange<ChemMasterComponent>(coords, 10).Count != 0
                    || IsPowerInfrastructureNearby(coords))
                    continue;

                if (!HasOpenPortalArea(grid, tilePos))
                    continue;

                var tooClose = false;
                foreach (var existingPosition in portalPositions)
                {
                    var deltaX = existingPosition.X - tilePos.X;
                    var deltaY = existingPosition.Y - tilePos.Y;

                    if (deltaX * deltaX + deltaY * deltaY < 15 * 15)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (tooClose)
                    continue;

                Spawn(entityPrototype, coords);
                portalPositions.Add(tilePos);
                component.PortalsRemaining++;
                break;
            }
        }
    }

    private bool IsPowerInfrastructureNearby(EntityCoordinates coordinates)
    {
        foreach (var entity in _lookup.GetEntitiesInRange(coordinates, 10))
        {
            if (!TryComp(entity, out MetaDataComponent? metadata)
                || metadata.EntityPrototype is not { } prototype)
            {
                continue;
            }

            var prototypeId = prototype.ID;
            if (prototypeId.StartsWith("SMES", StringComparison.Ordinal) || prototypeId.StartsWith("Substation", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private bool HasOpenPortalArea(EntityUid grid, Vector2i center)
    {
        var gridComponent = Comp<MapGridComponent>(grid);

        for (var x = -1; x <= 1; x++)
        {
            for (var y = -1; y <= 1; y++)
            {
                var tile = _mapsys.GetTileRef((grid, gridComponent), new Vector2i(
                    center.X + x,
                    center.Y + y));

                if (tile.Tile.IsEmpty)
                    return false;
                if (_turf.IsTileBlocked(tile, CollisionGroup.Impassable) || _turf.IsTileBlocked(tile, CollisionGroup.HighImpassable))
                    return false;
            }
        }

        return true;
    }

}