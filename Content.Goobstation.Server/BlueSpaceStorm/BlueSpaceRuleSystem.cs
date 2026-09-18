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

namespace Content.Server.StationEvents.Events;

public sealed partial class BlueSpaceStormRuleSystem :
    StationEventSystem<BlueSpaceRuleComponent>
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

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
            "Warning. Possible Bluespace phenomena detected, please alert security of any unusual activity.",
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

        foreach (EntProtoId entityPrototype in component.AvailablePortals)
        {
            for (var i = 0; i < 100; i++)
            {
                if (!TryFindRandomTile(
                        out var tilePos,
                        out var grid,
                        out _,
                        out var coords))
                {
                    continue;
                }

                if (grid != gridUid.Value)
                    continue;

                if (_lookup.GetEntitiesInRange<ChemMasterComponent>(coords, 10).Count != 0
                    || IsPowerInfrastructureNearby(coords))
                    continue;

                if (!HasOpenPortalArea(grid.Value, tilePos))
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
            }
        }

        return true;
    }

}