// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Client.Pinpointer.UI;
using Content.Client.Resources;
using Content.Shared.Atmos;
using Content.Shared.Medical.SuitSensor;
using Content.Shared.Movement.Components;
using Content.Shared.Pinpointer;
using Content.Shared.Silicons.StationAi;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.ResourceManagement;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Timing;

namespace Content.Client.Silicons.StationAi;

/// <summary>
/// Renders the crew monitoring map and crew sensor blips in the AI's viewport when zoomed out past normal max.
/// </summary>
public sealed class StationAiCrewMonitoringOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IResourceCache _cache = default!;

    private readonly SharedTransformSystem _transform;
    private readonly StationAiSystem _aiSystem;
    private readonly Font _font;
    private readonly Font _boldFont;

    public override OverlaySpace Space => OverlaySpace.WorldSpace | OverlaySpace.ScreenSpace;

    public StationAiCrewMonitoringOverlay()
    {
        IoCManager.InjectDependencies(this);
        _transform = _entManager.System<SharedTransformSystem>();
        _aiSystem = _entManager.System<StationAiSystem>();
        _font = _cache.GetFont("/Fonts/NotoSans/NotoSans-Regular.ttf", 10);
        _boldFont = _cache.GetFont("/Fonts/NotoSans/NotoSans-Bold.ttf", 11);
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        var player = _player.LocalEntity;
        if (player == null)
            return false;

        if (!_entManager.HasComponent<StationAiOverlayComponent>(player) &&
            !_entManager.HasComponent<StationAiHeldComponent>(player))
            return false;

        if (!_entManager.TryGetComponent<EyeComponent>(player, out var eye) || eye.Zoom.X <= 1.445f)
            return false;

        return true;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (args.Space == OverlaySpace.WorldSpace)
        {
            DrawWorld(args);
        }
        else if (args.Space == OverlaySpace.ScreenSpace)
        {
            DrawScreen(args);
        }
    }

    private void DrawWorld(in OverlayDrawArgs args)
    {
        var worldHandle = args.WorldHandle;

        // 1. Solid background covering the visible world to hide standard world rendering
        worldHandle.DrawRect(args.WorldBounds, new Color(20, 13, 26, 255));

        // 2. Render NavMap station layout
        var gridQuery = _entManager.EntityQueryEnumerator<NavMapComponent, MapGridComponent, TransformComponent>();
        while (gridQuery.MoveNext(out var gridUid, out var navMap, out var grid, out var xform))
        {
            if (xform.MapID != args.MapId)
                continue;

            var gridMatrix = _transform.GetWorldMatrix(gridUid);
            worldHandle.SetTransform(gridMatrix);

            var floorColor = new Color(55, 32, 60, 160);
            var wallColor = new Color(192, 122, 196, 255);
            var airlockColor = new Color(230, 160, 230, 255);

            var invMatrix = _transform.GetInvWorldMatrix(gridUid);
            var localBounds = invMatrix.TransformBox(args.WorldBounds);

            foreach (var (chunkOrigin, chunk) in navMap.Chunks)
            {
                var chunkWorldOrigin = (Vector2) (chunkOrigin * SharedNavMapSystem.ChunkSize) * grid.TileSize;
                var chunkBox = new Box2(chunkWorldOrigin, chunkWorldOrigin + new Vector2(SharedNavMapSystem.ChunkSize * grid.TileSize, SharedNavMapSystem.ChunkSize * grid.TileSize));
                if (!chunkBox.Intersects(localBounds))
                    continue;

                for (var i = 0; i < SharedNavMapSystem.ArraySize; i++)
                {
                    var tileData = chunk.TileData[i];
                    if (tileData == 0)
                        continue;

                    var relativeTile = SharedNavMapSystem.GetTileFromIndex(i);
                    var tile = (Vector2) (chunk.Origin * SharedNavMapSystem.ChunkSize + relativeTile) * grid.TileSize;
                    var tileBox = new Box2(tile, tile + new Vector2(grid.TileSize, grid.TileSize));

                    // Floors
                    if ((tileData & SharedNavMapSystem.FloorMask) != 0)
                    {
                        worldHandle.DrawRect(tileBox, floorColor);
                    }

                    // Airlocks
                    if ((tileData & SharedNavMapSystem.AirlockMask) != 0)
                    {
                        worldHandle.DrawRect(new Box2(tile + new Vector2(0.15f, 0.15f), tile + new Vector2(0.85f, 0.85f)), airlockColor);
                    }
                    // Walls
                    else if ((tileData & SharedNavMapSystem.WallMask) != 0)
                    {
                        var wallDirData = (tileData & SharedNavMapSystem.WallMask) >> (int) NavMapChunkType.Wall;
                        if (wallDirData == SharedNavMapSystem.AllDirMask)
                        {
                            worldHandle.DrawRect(tileBox, wallColor);
                        }
                        else
                        {
                            if ((wallDirData & (int) AtmosDirection.South) != 0)
                                worldHandle.DrawLine(tile, tile + new Vector2(grid.TileSize, 0), wallColor);
                            if ((wallDirData & (int) AtmosDirection.North) != 0)
                                worldHandle.DrawLine(tile + new Vector2(0, grid.TileSize), tile + new Vector2(grid.TileSize, grid.TileSize), wallColor);
                            if ((wallDirData & (int) AtmosDirection.East) != 0)
                                worldHandle.DrawLine(tile + new Vector2(grid.TileSize, 0), tile + new Vector2(grid.TileSize, grid.TileSize), wallColor);
                            if ((wallDirData & (int) AtmosDirection.West) != 0)
                                worldHandle.DrawLine(tile, tile + new Vector2(0, grid.TileSize), wallColor);
                        }
                    }
                }
            }
        }
        worldHandle.SetTransform(Matrix3x2.Identity);

        // 3. Crew member blips
        foreach (var sensor in _aiSystem.ActiveCrewSensors)
        {
            if (sensor.Coordinates is not { } netCoords)
                continue;

            var mapCoords = _transform.ToMapCoordinates(netCoords);
            if (mapCoords.MapId != args.MapId)
                continue;

            var pos = mapCoords.Position;
            if (!args.WorldBounds.Contains(pos))
                continue;

            Color blipColor;
            if (!sensor.IsAlive)
                blipColor = Color.Red;
            else if (sensor.TotalDamage > 0)
                blipColor = Color.Orange;
            else
                blipColor = Color.FromHex("#48d38a");

            worldHandle.DrawCircle(pos, 0.5f, blipColor);
            worldHandle.DrawCircle(pos, 0.22f, Color.White);
        }

        // 4. AI Eye blip
        var playerEnt = _player.LocalEntity;
        if (playerEnt != null &&
            _aiSystem.TryGetCore(playerEnt.Value, out var core) &&
            core.Comp?.RemoteEntity is { } holo &&
            _entManager.EntityExists(holo))
        {
            var holoMapPos = _transform.GetMapCoordinates(holo);
            if (holoMapPos.MapId == args.MapId && args.WorldBounds.Contains(holoMapPos.Position))
            {
                var pulse = 0.55f + 0.1f * MathF.Sin((float) _timing.RealTime.TotalSeconds * 6f);
                worldHandle.DrawCircle(holoMapPos.Position, pulse, Color.FromHex("#00e5ff"));
                worldHandle.DrawCircle(holoMapPos.Position, 0.25f, Color.White);
            }
        }
    }

    private void DrawScreen(in OverlayDrawArgs args)
    {
        if (args.ViewportControl == null)
            return;

        var screenHandle = args.ScreenHandle;
        var viewportBounds = UIBox2.FromDimensions(Vector2.Zero, args.Viewport.Size);

        // 1. Room beacons
        var gridQuery = _entManager.EntityQueryEnumerator<NavMapComponent, TransformComponent>();
        while (gridQuery.MoveNext(out var gridUid, out var navMap, out var xform))
        {
            if (xform.MapID != args.MapId)
                continue;

            var gridMatrix = _transform.GetWorldMatrix(gridUid);
            foreach (var beacon in navMap.Beacons.Values)
            {
                var beaconWorldPos = Vector2.Transform(beacon.Position, gridMatrix);
                var screenPos = args.ViewportControl.WorldToScreen(beaconWorldPos);
                if (!viewportBounds.Contains(screenPos))
                    continue;

                screenHandle.DrawString(_font, screenPos + new Vector2(4, -5), beacon.Text, beacon.Color.WithAlpha(0.7f));
            }
        }

        // 2. Crew names and jobs
        foreach (var sensor in _aiSystem.ActiveCrewSensors)
        {
            if (sensor.Coordinates is not { } netCoords)
                continue;

            var mapCoords = _transform.ToMapCoordinates(netCoords);
            if (mapCoords.MapId != args.MapId)
                continue;

            var screenPos = args.ViewportControl.WorldToScreen(mapCoords.Position);
            if (!viewportBounds.Contains(screenPos))
                continue;

            var label = $"{sensor.Name} ({sensor.Job})";
            var textSize = screenHandle.GetDimensions(_font, label, 1f);
            var labelPos = screenPos + new Vector2(10, -textSize.Y / 2);
            screenHandle.DrawRect(new UIBox2(labelPos - new Vector2(2, 1), labelPos + textSize + new Vector2(2, 1)), new Color(10, 10, 15, 200));
            screenHandle.DrawString(_font, labelPos, label, Color.White);
        }

        // 3. AI Eye label
        var playerEnt = _player.LocalEntity;
        if (playerEnt != null &&
            _aiSystem.TryGetCore(playerEnt.Value, out var core) &&
            core.Comp?.RemoteEntity is { } holo &&
            _entManager.EntityExists(holo))
        {
            var holoMapPos = _transform.GetMapCoordinates(holo);
            if (holoMapPos.MapId == args.MapId)
            {
                var screenPos = args.ViewportControl.WorldToScreen(holoMapPos.Position);
                if (viewportBounds.Contains(screenPos))
                {
                    var isHidden = _entManager.TryGetComponent<StationAiHeldComponent>(playerEnt, out var held) && !held.EyeVisibleOnCrewMonitor;
                    var eyeLabel = isHidden ? "AI EYE (HIDDEN)" : "AI EYE";
                    var eyeColor = isHidden ? Color.FromHex("#70a0c0") : Color.FromHex("#00e5ff");
                    var textSize = screenHandle.GetDimensions(_boldFont, eyeLabel, 1f);
                    var labelPos = screenPos + new Vector2(12, -textSize.Y / 2);
                    screenHandle.DrawRect(new UIBox2(labelPos - new Vector2(3, 2), labelPos + textSize + new Vector2(3, 2)), new Color(5, 15, 25, 220));
                    screenHandle.DrawString(_boldFont, labelPos, eyeLabel, eyeColor);
                }
            }
        }
    }
}
