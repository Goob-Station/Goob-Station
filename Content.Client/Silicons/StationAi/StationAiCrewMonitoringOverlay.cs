// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Client.Pinpointer.UI;
using Content.Client.Resources;
using Content.Shared.Atmos;
using Content.Shared.Medical.SuitSensor;
using Content.Shared.Movement.Components;
using Content.Shared.Pinpointer;
using Content.Shared.Shuttles.Components;
using Content.Shared.Shuttles.Systems;
using Content.Shared.Silicons.StationAi;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.ResourceManagement;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Timing;

namespace Content.Client.Silicons.StationAi;

/// <summary>
/// Renders the crew monitoring map (with camera reach highlights) and mass scanner radar in the AI's viewport when zoomed out.
/// </summary>
public sealed class StationAiCrewMonitoringOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IResourceCache _cache = default!;

    private readonly SharedTransformSystem _transform;
    private readonly StationAiSystem _aiSystem;
    private readonly StationAiVisionSystem _vision;
    private readonly SharedShuttleSystem _shuttleSystem;
    private readonly Font _font;
    private readonly Font _boldFont;

    private readonly Dictionary<EntityUid, HashSet<Vector2i>> _gridVisibleTiles = new();
    private float _visionAccumulator = 0f;
    private const float VisionUpdateRate = 0.15f;

    public const float CrewMonitoringZoomThreshold = SharedStationAiSystem.CrewMonitoringZoomThreshold;
    public const float MassScannerZoomThreshold = SharedStationAiSystem.MassScannerZoomThreshold;

    private static readonly float[] RadarRangeRadii = { 50f, 100f, 200f, 350f, 500f, 750f, 1000f, 1500f };

    public override OverlaySpace Space => OverlaySpace.WorldSpace | OverlaySpace.ScreenSpace;

    public StationAiCrewMonitoringOverlay()
    {
        IoCManager.InjectDependencies(this);
        _transform = _entManager.System<SharedTransformSystem>();
        _aiSystem = _entManager.System<StationAiSystem>();
        _vision = _entManager.System<StationAiVisionSystem>();
        _shuttleSystem = _entManager.System<SharedShuttleSystem>();
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

        if (!_entManager.TryGetComponent<EyeComponent>(player, out var eye) || eye.Zoom.X <= CrewMonitoringZoomThreshold)
            return false;

        return true;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var player = _player.LocalEntity;
        if (player == null || !_entManager.TryGetComponent<EyeComponent>(player, out var eye))
            return;

        var isMassScanner = eye.Zoom.X > MassScannerZoomThreshold;

        if (args.Space == OverlaySpace.WorldSpace)
        {
            if (isMassScanner)
                DrawMassScannerWorld(args);
            else
                DrawCrewMonitoringWorld(args);
        }
        else if (args.Space == OverlaySpace.ScreenSpace)
        {
            if (isMassScanner)
                DrawMassScannerScreen(args, eye);
            else
                DrawCrewMonitoringScreen(args);
        }
    }

    private void DrawCrewMonitoringWorld(in OverlayDrawArgs args)
    {
        var worldHandle = args.WorldHandle;

        // 1. Solid background covering the visible world to hide standard world rendering
        worldHandle.DrawRect(args.WorldBounds, new Color(20, 13, 26, 255));

        // Update camera reach cache every 0.15s
        _visionAccumulator -= (float) _timing.FrameTime.TotalSeconds;
        var refreshVision = false;
        if (_visionAccumulator <= 0f)
        {
            _visionAccumulator = VisionUpdateRate;
            refreshVision = true;
        }

        // 2. Render NavMap station layout with camera reach indicator
        var gridQuery = _entManager.EntityQueryEnumerator<NavMapComponent, MapGridComponent, TransformComponent>();
        while (gridQuery.MoveNext(out var gridUid, out var navMap, out var grid, out var xform))
        {
            if (xform.MapID != args.MapId)
                continue;

            if (!_gridVisibleTiles.TryGetValue(gridUid, out var visibleSet))
            {
                visibleSet = new HashSet<Vector2i>();
                _gridVisibleTiles[gridUid] = visibleSet;
                if (_entManager.TryGetComponent<BroadphaseComponent>(gridUid, out var bp))
                    _vision.GetView((gridUid, bp, grid), args.WorldBounds, visibleSet);
            }
            else if (refreshVision)
            {
                visibleSet.Clear();
                if (_entManager.TryGetComponent<BroadphaseComponent>(gridUid, out var bp))
                    _vision.GetView((gridUid, bp, grid), args.WorldBounds, visibleSet);
            }

            var gridMatrix = _transform.GetWorldMatrix(gridUid);
            worldHandle.SetTransform(gridMatrix);

            var floorColor = new Color(55, 32, 60, 160);
            var floorCameraColor = new Color(100, 58, 115, 220); // Noticeably brighter tint for tiles in camera reach
            var wallColor = new Color(192, 122, 196, 255);
            var airlockColor = new Color(230, 160, 230, 255);
            var airlockCameraColor = new Color(255, 205, 255, 255);

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
                    var tileIndices = chunk.Origin * SharedNavMapSystem.ChunkSize + relativeTile;
                    var inCameraReach = visibleSet.Contains(tileIndices);
                    var tile = (Vector2) tileIndices * grid.TileSize;
                    var tileBox = new Box2(tile, tile + new Vector2(grid.TileSize, grid.TileSize));

                    // Floors
                    if ((tileData & SharedNavMapSystem.FloorMask) != 0)
                    {
                        worldHandle.DrawRect(tileBox, inCameraReach ? floorCameraColor : floorColor);
                    }

                    // Airlocks
                    if ((tileData & SharedNavMapSystem.AirlockMask) != 0)
                    {
                        worldHandle.DrawRect(new Box2(tile + new Vector2(0.15f, 0.15f), tile + new Vector2(0.85f, 0.85f)), inCameraReach ? airlockCameraColor : airlockColor);
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

    private void DrawMassScannerWorld(in OverlayDrawArgs args)
    {
        var worldHandle = args.WorldHandle;

        // 1. Deep space radar background
        worldHandle.DrawRect(args.WorldBounds, new Color(8, 12, 16, 255));

        // 2. Determine radar origin (AI eye world coordinates)
        var eyePos = args.WorldAABB.Center;
        var playerEnt = _player.LocalEntity;
        if (playerEnt != null &&
            _aiSystem.TryGetCore(playerEnt.Value, out var core) &&
            core.Comp?.RemoteEntity is { } holo &&
            _entManager.EntityExists(holo))
        {
            var holoMapPos = _transform.GetMapCoordinates(holo);
            if (holoMapPos.MapId == args.MapId)
                eyePos = holoMapPos.Position;
        }

        // 3. Concentric range rings and cardinal radar axes
        var ringColor = new Color(0, 229, 255, 30);
        foreach (var radius in RadarRangeRadii)
        {
            worldHandle.DrawCircle(eyePos, radius, ringColor, false);
        }

        var axisColor = new Color(0, 229, 255, 20);
        worldHandle.DrawLine(eyePos - new Vector2(1600f, 0), eyePos + new Vector2(1600f, 0), axisColor);
        worldHandle.DrawLine(eyePos - new Vector2(0, 1600f), eyePos + new Vector2(0, 1600f), axisColor);

        // 4. Sector Grids (Mass contacts) - NO camera reach tinting!
        _entManager.TryGetComponent(playerEnt, out TransformComponent? playerXform);
        var ourGridId = playerXform?.GridUid;

        var gridQuery = _entManager.EntityQueryEnumerator<MapGridComponent, TransformComponent>();
        while (gridQuery.MoveNext(out var gridUid, out var grid, out var xform))
        {
            if (xform.MapID != args.MapId || gridUid == args.MapUid)
                continue;

            _entManager.TryGetComponent<IFFComponent>(gridUid, out var iff);
            if (iff != null && (iff.Flags & IFFFlags.Hide) != 0)
                continue;

            var isSelf = (ourGridId != null && ourGridId == gridUid);
            var iffColor = _shuttleSystem.GetIFFColor(gridUid, self: isSelf, iff);

            var gridMatrix = _transform.GetWorldMatrix(gridUid);
            worldHandle.SetTransform(gridMatrix);

            var invMatrix = _transform.GetInvWorldMatrix(gridUid);
            var localBounds = invMatrix.TransformBox(args.WorldBounds);

            if (_entManager.TryGetComponent<NavMapComponent>(gridUid, out var navMap))
            {
                var floorColor = iffColor.WithAlpha(0.28f);
                var wallColor = iffColor.WithAlpha(0.85f);

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

                        if ((tileData & SharedNavMapSystem.FloorMask) != 0)
                        {
                            worldHandle.DrawRect(tileBox, floorColor);
                        }

                        if ((tileData & SharedNavMapSystem.AirlockMask) != 0)
                        {
                            worldHandle.DrawRect(new Box2(tile + new Vector2(0.15f, 0.15f), tile + new Vector2(0.85f, 0.85f)), wallColor);
                        }
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
            else
            {
                worldHandle.DrawRect(grid.LocalAABB, iffColor.WithAlpha(0.25f));
                worldHandle.DrawRect(grid.LocalAABB, iffColor.WithAlpha(0.75f), false);
            }
        }
        worldHandle.SetTransform(Matrix3x2.Identity);

        // 5. AI Eye reticle marker
        if (args.WorldBounds.Contains(eyePos))
        {
            var pulse = 0.55f + 0.15f * MathF.Sin((float) _timing.RealTime.TotalSeconds * 5f);
            worldHandle.DrawCircle(eyePos, pulse * 1.5f, new Color(0, 229, 255, 120), false);
            worldHandle.DrawCircle(eyePos, 0.35f, Color.White);
        }
    }

    private void DrawCrewMonitoringScreen(in OverlayDrawArgs args)
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

    private void DrawMassScannerScreen(in OverlayDrawArgs args, EyeComponent eye)
    {
        if (args.ViewportControl == null)
            return;

        var screenHandle = args.ScreenHandle;
        var viewportBounds = UIBox2.FromDimensions(Vector2.Zero, args.Viewport.Size);

        var eyePos = args.WorldAABB.Center;
        var playerEnt = _player.LocalEntity;
        if (playerEnt != null &&
            _aiSystem.TryGetCore(playerEnt.Value, out var core) &&
            core.Comp?.RemoteEntity is { } holo &&
            _entManager.EntityExists(holo))
        {
            var holoMapPos = _transform.GetMapCoordinates(holo);
            if (holoMapPos.MapId == args.MapId)
                eyePos = holoMapPos.Position;
        }

        // 1. Range ring distance labels
        foreach (var radius in RadarRangeRadii)
        {
            var screenPos = args.ViewportControl.WorldToScreen(eyePos + new Vector2(0, -radius));
            if (viewportBounds.Contains(screenPos))
            {
                var text = $"{radius:0}m";
                screenHandle.DrawString(_font, screenPos + new Vector2(6, -6), text, new Color(0, 229, 255, 160));
            }
        }

        // 2. Sector grid contact labels
        _entManager.TryGetComponent(playerEnt, out TransformComponent? playerXform);
        var ourGridId = playerXform?.GridUid;

        var contactCount = 0;
        var gridQuery = _entManager.EntityQueryEnumerator<MapGridComponent, TransformComponent>();
        while (gridQuery.MoveNext(out var gridUid, out _, out var xform))
        {
            if (xform.MapID != args.MapId || gridUid == args.MapUid)
                continue;

            _entManager.TryGetComponent<IFFComponent>(gridUid, out var iff);
            if (iff != null && (iff.Flags & IFFFlags.Hide) != 0)
                continue;

            contactCount++;

            var isSelf = (ourGridId != null && ourGridId == gridUid);
            var iffLabel = _shuttleSystem.GetIFFLabel(gridUid, self: isSelf, iff);
            if (iffLabel == null)
                continue;

            var iffColor = _shuttleSystem.GetIFFColor(gridUid, self: isSelf, iff);
            var gridPos = _transform.GetWorldPosition(gridUid);
            var distance = (gridPos - eyePos).Length();

            var screenPos = args.ViewportControl.WorldToScreen(gridPos);
            var isOffscreen = !viewportBounds.Contains(screenPos);

            var contactText = $"[ {iffLabel} ]  {distance:0}m";
            var coordsText = $"({gridPos.X:0}, {gridPos.Y:0})";

            var labelDims = screenHandle.GetDimensions(_boldFont, contactText, 1f);
            var coordDims = screenHandle.GetDimensions(_font, coordsText, 0.85f);
            var boxWidth = MathF.Max(labelDims.X, coordDims.X) + 12f;
            var boxHeight = labelDims.Y + coordDims.Y + 6f;

            Vector2 drawPos;
            if (isOffscreen)
            {
                var center = args.Viewport.Size / 2f;
                var dir = screenPos - center;
                if (dir.LengthSquared() < 0.001f)
                    continue;

                var normDir = dir.Normalized();
                var halfSize = args.Viewport.Size / 2f - new Vector2(boxWidth / 2f + 25f, boxHeight / 2f + 25f);
                var scaleX = halfSize.X / MathF.Abs(normDir.X);
                var scaleY = halfSize.Y / MathF.Abs(normDir.Y);
                var scale = MathF.Min(scaleX, scaleY);
                drawPos = center + normDir * scale - new Vector2(boxWidth / 2f, boxHeight / 2f);
            }
            else
            {
                drawPos = screenPos + new Vector2(16, -boxHeight / 2f);
            }

            var box = new UIBox2(drawPos, drawPos + new Vector2(boxWidth, boxHeight));
            screenHandle.DrawRect(box, new Color(6, 12, 18, 220));
            screenHandle.DrawRect(box, iffColor.WithAlpha(0.6f), false);
            screenHandle.DrawString(_boldFont, drawPos + new Vector2(6, 2), contactText, iffColor);
            screenHandle.DrawString(_font, drawPos + new Vector2(6, labelDims.Y + 2), coordsText, iffColor.WithAlpha(0.75f));
        }

        // 3. Top HUD Banner
        var bannerTitle = "MASS SCANNER // SECTOR RADAR";
        var bannerSub = $"CONTACTS: {contactCount} | ORIGIN: ({eyePos.X:0}, {eyePos.Y:0}) | ZOOM: {eye.Zoom.X:0.0}x";
        var titleDims = screenHandle.GetDimensions(_boldFont, bannerTitle, 1f);
        var subDims = screenHandle.GetDimensions(_font, bannerSub, 0.85f);
        var bannerWidth = MathF.Max(titleDims.X, subDims.X) + 20f;
        var bannerHeight = titleDims.Y + subDims.Y + 10f;

        var bannerPos = new Vector2(20, 20);
        var bannerBox = new UIBox2(bannerPos, bannerPos + new Vector2(bannerWidth, bannerHeight));
        screenHandle.DrawRect(bannerBox, new Color(6, 12, 18, 225));
        screenHandle.DrawRect(bannerBox, new Color(0, 229, 255, 140), false);
        screenHandle.DrawString(_boldFont, bannerPos + new Vector2(10, 4), bannerTitle, Color.FromHex("#00e5ff"));
        screenHandle.DrawString(_font, bannerPos + new Vector2(10, titleDims.Y + 4), bannerSub, new Color(0, 229, 255, 180));

        // 4. AI Eye label
        var eyeScreenPos = args.ViewportControl.WorldToScreen(eyePos);
        if (viewportBounds.Contains(eyeScreenPos))
        {
            var isHidden = _entManager.TryGetComponent<StationAiHeldComponent>(playerEnt, out var held) && !held.EyeVisibleOnCrewMonitor;
            var eyeLabel = isHidden ? "AI EYE [RADAR ORIGIN - HIDDEN]" : "AI EYE [RADAR ORIGIN]";
            var eyeColor = isHidden ? Color.FromHex("#70a0c0") : Color.FromHex("#00e5ff");
            var textSize = screenHandle.GetDimensions(_boldFont, eyeLabel, 1f);
            var labelPos = eyeScreenPos + new Vector2(14, -textSize.Y / 2);
            screenHandle.DrawRect(new UIBox2(labelPos - new Vector2(3, 2), labelPos + textSize + new Vector2(3, 2)), new Color(6, 12, 18, 220));
            screenHandle.DrawString(_boldFont, labelPos, eyeLabel, eyeColor);
        }
    }
}
