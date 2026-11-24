using System.Numerics;
using Content.Goobstation.Shared.Nullspace;
using Content.Shared.Maps;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Graphics;
using Robust.Shared.Graphics.RSI;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.Nullspace;

public sealed class NullspaceRenderer : IDisposable
{
    private const float Cell = NullspaceParams.Cell;
    private const float WallHeight = NullspaceParams.WallHeight;
    private const int RenderHeight = 240;
    private const int BakeSize = 64;
    private const int ViewRadius = 11;
    private const float NearPlane = 0.05f;
    private const float WallBottom = -0.1f;
    private const float FixtureHeight = WallHeight - 0.055f;
    private const float FixtureWidth = 0.22f;
    private const float FixtureLength = 1.3f;
    private const float FogDensity = 0.085f;

    private const float MaterialFloor = 0f;
    private const float MaterialCeiling = 1f;
    private const float MaterialWall = 2f;
    private const float MaterialGlowingBillboard = 6f;
    private const float MaterialEmissive = 7f;
    private const float MaterialBillboard = 8f;
    private const float LampReach = 5.8f / NullspaceParams.Cell + 1f;
    private const float SpotReach = 24f / NullspaceParams.Cell + 1f;

    private const int WallFrameSize = EyeManager.PixelsPerMeter;
    private const string WallRsi = "/Textures/Structures/Walls/solid.rsi";
    private const string WallState = "solid";

    private static readonly string[] SpotNames = { "spot0", "spot1", "spot2", "spot3" };
    private static readonly string[] SpotDirectionNames = { "spot0Dir", "spot1Dir", "spot2Dir", "spot3Dir" };
    private static readonly ProtoId<ShaderPrototype> SurfaceShader = "NullspaceSurface";
    private static readonly ProtoId<ShaderPrototype> PostShader = "NullspacePost";
    private static readonly ProtoId<ContentTileDefinition> FloorTile = "Plating";
    private static readonly ProtoId<ContentTileDefinition> CeilingTile = "FloorHull";
    private static readonly Color FogColor = Color.FromHex("#020203");
    private static readonly Color BakeClear = Color.Black.WithAlpha(0f);
    private static readonly Vector3 FixtureColor = new(0.86f, 0.95f, 0.88f);
    private static readonly Vector3 LampColor = SrgbToLinear(0xdf, 0xee, 0xdd);
    private static readonly Vector3 SpotColor = SrgbToLinear(0xff, 0xf0, 0xd8);
    private static readonly Vector3 Ambient = SrgbToLinear(0x88, 0x90, 0xa0) * 0.12f;

    private readonly IClyde _clyde;
    private readonly IEntityManager _entMan;
    private readonly IResourceCache _resources;
    private readonly ShaderInstance _surface;
    private readonly ShaderInstance _post;
    private readonly Texture _floor;
    private readonly Texture _ceiling;
    private readonly float _floorU;
    private readonly float _ceilingU;
    private readonly IRenderTexture _walls;
    private readonly List<IRenderTexture> _bakes = new();
    private readonly List<bool> _baked = new();
    private IRenderTexture? _scene;
    private bool _wallsBaked;

    private readonly List<Vector2i> _open = new();
    private readonly List<(float Distance, Vector2i Cell, int Billboard)> _sorted = new();
    private readonly Dictionary<Vector2i, int> _masks = new();
    private static readonly Vector2[] VisibilitySamples =
    {
        new(0f, 0f), new(-0.4f, -0.4f), new(0.4f, -0.4f), new(-0.4f, 0.4f), new(0.4f, 0.4f),
    };

    private readonly ClipVertex[] _clipIn = new ClipVertex[8];
    private readonly ClipVertex[] _clipOut = new ClipVertex[8];
    private DrawVertexUV2DColor[] _batch = new DrawVertexUV2DColor[4096];
    private int _batchCount;
    private Texture? _batchTexture;

    private NullspaceFrame _frame = default!;
    private ShaderInstance? _outerShader;
    private float _width;
    private float _height;
    private float _aspect;
    private float _tanX;
    private float _tanY;

    public NullspaceRenderer(
        IClyde clyde,
        IEntityManager entMan,
        IPrototypeManager prototypes,
        IResourceCache resources,
        ITileDefinitionManager tiles)
    {
        _clyde = clyde;
        _entMan = entMan;
        _resources = resources;
        _surface = prototypes.Index(SurfaceShader).InstanceUnique();
        _post = prototypes.Index(PostShader).InstanceUnique();
        _floor = TileTexture(tiles, FloorTile, out _floorU);
        _ceiling = TileTexture(tiles, CeilingTile, out _ceilingU);
        _walls = clyde.CreateRenderTarget(
            new Vector2i(WallFrameSize * 4, WallFrameSize),
            new RenderTargetFormatParameters(RenderTargetColorFormat.Rgba8Srgb),
            new TextureSampleParameters { Filter = false },
            "nullspace-walls");
    }

    public void Dispose()
    {
        _walls.Dispose();
        foreach (var bake in _bakes)
        {
            bake.Dispose();
        }

        _bakes.Clear();
        _scene?.Dispose();
        _scene = null;
    }

    public void Draw(IRenderHandle renderHandle, Vector2i pixelSize, NullspaceFrame frame)
    {
        if (frame.Maze == null || pixelSize.X <= 0 || pixelSize.Y <= 0)
            return;

        var handle = renderHandle.DrawingHandleScreen;

        var scene = EnsureScene(pixelSize);
        var transform = handle.GetTransform();
        _outerShader = handle.GetShader();

        if (!_wallsBaked)
        {
            _wallsBaked = true;
            handle.RenderInRenderTarget(_walls,
                () =>
                {
                    renderHandle.SetScissor(null);
                    handle.SetTransform(Matrix3x2.Identity);
                    BakeWallFaces(handle);
                    handle.UseShader(_outerShader);
                },
                BakeClear);
        }

        for (var i = 0; i < frame.Billboards.Count; i++)
        {
            var billboard = frame.Billboards[i];
            var bake = EnsureBake(i);
            _baked[i] = false;
            if (_entMan.Deleted(billboard.Entity)
                || !_entMan.TryGetComponent(billboard.Entity, out SpriteComponent? sprite))
            {
                continue;
            }

            _baked[i] = true;
            handle.RenderInRenderTarget(bake,
                () =>
                {
                    renderHandle.SetScissor(null);
                    handle.SetTransform(Matrix3x2.Identity);
                    handle.DrawEntity(billboard.Entity,
                        new Vector2(BakeSize / 2f, BakeSize / 2f),
                        new Vector2(BakeSize / (float) EyeManager.PixelsPerMeter),
                        Angle.Zero,
                        Angle.Zero,
                        billboard.Direction,
                        sprite);
                    handle.UseShader(_outerShader);
                },
                BakeClear);
        }

        handle.RenderInRenderTarget(scene,
            () =>
            {
                renderHandle.SetScissor(null);
                handle.SetTransform(Matrix3x2.Identity);
                DrawScene(handle, frame, scene.Size);
            },
            FogColor);

        handle.SetTransform(transform);

        _post.SetParameter("res", (Vector2) scene.Size);
        _post.SetParameter("time", frame.Time);
        _post.SetParameter("fear", frame.Fear);
        _post.SetParameter("exhaustion", frame.Exhaustion);
        _post.SetParameter("flash", frame.Flash);
        _post.SetParameter("fadeIn", frame.FadeIn);

        handle.UseShader(_post);
        handle.DrawTextureRect(scene.Texture, UIBox2.FromDimensions(Vector2.Zero, pixelSize));
        handle.UseShader(_outerShader);
    }

    private IRenderTexture EnsureBake(int index)
    {
        while (_bakes.Count <= index)
        {
            _bakes.Add(_clyde.CreateRenderTarget(
                new Vector2i(BakeSize, BakeSize),
                new RenderTargetFormatParameters(RenderTargetColorFormat.Rgba8Srgb),
                new TextureSampleParameters { Filter = false },
                "nullspace-billboard"));
            _baked.Add(false);
        }

        return _bakes[index];
    }

    private IRenderTexture EnsureScene(Vector2i pixelSize)
    {
        var width = Math.Max(1, (int) MathF.Round(RenderHeight * pixelSize.X / (float) pixelSize.Y));
        var size = new Vector2i(width, RenderHeight);
        if (_scene != null && _scene.Size == size)
            return _scene;

        _scene?.Dispose();
        _scene = _clyde.CreateRenderTarget(
            size,
            new RenderTargetFormatParameters(RenderTargetColorFormat.Rgba16F),
            new TextureSampleParameters { Filter = false },
            "nullspace-scene");
        return _scene;
    }

    private void DrawScene(DrawingHandleScreen handle, NullspaceFrame frame, Vector2i size)
    {
        var maze = frame.Maze!;
        _frame = frame;
        _width = size.X;
        _height = size.Y;
        _aspect = _width / _height;
        _tanX = _aspect * 0.5f / frame.Focal;
        _tanY = 0.5f / frame.Focal;
        _batchCount = 0;
        _batchTexture = null;

        _masks.Clear();
        SetSurfaceParameters(frame);
        handle.UseShader(_surface);

        var camera = frame.CameraPosition;
        var cameraCell = new Vector2i((int) MathF.Floor(camera.X / Cell), (int) MathF.Floor(camera.Y / Cell));

        _open.Clear();
        _sorted.Clear();
        for (var dy = -ViewRadius; dy <= ViewRadius; dy++)
        {
            for (var dx = -ViewRadius; dx <= ViewRadius; dx++)
            {
                var cell = cameraCell + new Vector2i(dx, dy);
                if (!CellVisible(cell))
                    continue;

                if (!maze.IsWall(cell))
                {
                    _open.Add(cell);
                    continue;
                }

                var center = SharedNullspaceSystem.CellCenter(cell) - new Vector2(camera.X, camera.Y);
                _sorted.Add((center.LengthSquared(), cell, -1));
            }
        }

        foreach (var cell in _open)
        {
            var x0 = cell.X * Cell;
            var y0 = cell.Y * Cell;
            EmitQuad(handle, _floor,
                new Vector3(x0, y0, 0f), new Vector3(x0 + Cell, y0, 0f),
                new Vector3(x0 + Cell, y0 + Cell, 0f), new Vector3(x0, y0 + Cell, 0f),
                new Vector2(0f, 0f), new Vector2(_floorU, 0f), new Vector2(_floorU, 1f), new Vector2(0f, 1f),
                Pack(MaterialFloor, Mask(maze, cell)), Color.White);
        }

        foreach (var cell in _open)
        {
            var x0 = cell.X * Cell;
            var y0 = cell.Y * Cell;
            EmitQuad(handle, _ceiling,
                new Vector3(x0, y0, WallHeight), new Vector3(x0, y0 + Cell, WallHeight),
                new Vector3(x0 + Cell, y0 + Cell, WallHeight), new Vector3(x0 + Cell, y0, WallHeight),
                new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(_ceilingU, 1f), new Vector2(_ceilingU, 0f),
                Pack(MaterialCeiling, Mask(maze, cell)), Color.White);
        }

        foreach (var fixture in frame.Fixtures)
        {
            if (!CellVisible(fixture.Cell))
                continue;

            EmitFixture(handle, fixture);
        }

        for (var i = 0; i < frame.Billboards.Count; i++)
        {
            var billboard = frame.Billboards[i];
            if (billboard.InFace || !_baked[i])
                continue;

            var rel = new Vector2(billboard.Center.X - camera.X, billboard.Center.Y - camera.Y);
            _sorted.Add((rel.LengthSquared(), default, i));
        }

        _sorted.Sort(static (a, b) => b.Distance.CompareTo(a.Distance));

        foreach (var (_, cell, billboard) in _sorted)
        {
            if (billboard >= 0)
                EmitBillboard(handle, maze, billboard);
            else
                EmitWalls(handle, maze, cell);
        }

        for (var i = 0; i < frame.Billboards.Count; i++)
        {
            if (frame.Billboards[i].InFace && _baked[i])
                EmitBillboard(handle, maze, i);
        }

        Flush(handle);
        handle.UseShader(_outerShader);
    }

    private void SetSurfaceParameters(NullspaceFrame frame)
    {
        _surface.SetParameter("camPos", frame.CameraPosition);
        _surface.SetParameter("camForward", frame.CameraForward);
        _surface.SetParameter("camRight", frame.CameraRight);
        _surface.SetParameter("camUp", frame.CameraUp);
        _surface.SetParameter("focal", frame.Focal);
        _surface.SetParameter("aspect", _aspect);
        _surface.SetParameter("ambient", Ambient);
        _surface.SetParameter("lampColor", LampColor);
        _surface.SetParameter("lamp0", frame.Lamps[0]);
        _surface.SetParameter("lamp1", frame.Lamps[1]);
        _surface.SetParameter("lamp2", frame.Lamps[2]);
        _surface.SetParameter("lamp3", frame.Lamps[3]);
        _surface.SetParameter("spotColor", SpotColor);
        _surface.SetParameter("spotCone", new Vector2(MathF.Cos(0.5f), MathF.Cos(0.5f * (1f - 0.55f))));
        for (var i = 0; i < NullspaceFrame.MaxSpots; i++)
        {
            var spot = i < frame.Spots.Count ? frame.Spots[i] : default;
            _surface.SetParameter(SpotNames[i], new Vector4(spot.Position, spot.Intensity));
            _surface.SetParameter(SpotDirectionNames[i], spot.Direction);
        }
        _surface.SetParameter("fogColor", SrgbToLinear(0x02, 0x02, 0x03));
        _surface.SetParameter("fogDensity", FogDensity);
    }

    private void EmitWalls(DrawingHandleScreen handle, NullspaceMaze maze, Vector2i cell)
    {
        var camera = _frame.CameraPosition;
        for (var i = 0; i < NullspaceMaze.Directions.Length; i++)
        {
            var dir = NullspaceMaze.Directions[i];
            if (maze.IsWall(cell + dir))
                continue;

            var facing = dir.X switch
            {
                1 => camera.X > (cell.X + 1) * Cell,
                -1 => camera.X < cell.X * Cell,
                _ => dir.Y == 1 ? camera.Y > (cell.Y + 1) * Cell : camera.Y < cell.Y * Cell,
            };

            if (!facing)
                continue;

            var right = new Vector2i(-dir.Y, dir.X);
            var leftEnd = !maze.IsWall(cell - right);
            var rightEnd = !maze.IsWall(cell + right);
            var frame = leftEnd && rightEnd ? 3 : leftEnd ? 1 : rightEnd ? 2 : 0;
            var u0 = frame / 4f + 0.0005f;
            var u1 = (frame + 1) / 4f - 0.0005f;

            var mid = new Vector2((cell.X + 0.5f + dir.X * 0.5f) * Cell, (cell.Y + 0.5f + dir.Y * 0.5f) * Cell);
            var half = new Vector2(right.X, right.Y) * (Cell * 0.5f);
            var a = mid - half;
            var b = mid + half;

            EmitQuad(handle, _walls.Texture,
                new Vector3(a, WallBottom), new Vector3(b, WallBottom),
                new Vector3(b, WallHeight), new Vector3(a, WallHeight),
                new Vector2(u0, 0f), new Vector2(u1, 0f), new Vector2(u1, 1f), new Vector2(u0, 1f),
                Pack(MaterialWall + i, Mask(maze, cell + dir)), Color.White);
        }
    }

    private void EmitFixture(DrawingHandleScreen handle, NullspaceFixtureVisual fixture)
    {
        var center = SharedNullspaceSystem.CellCenter(fixture.Cell);
        var half = fixture.AlongX
            ? new Vector2(FixtureLength, FixtureWidth) * 0.5f
            : new Vector2(FixtureWidth, FixtureLength) * 0.5f;
        var glow = FixtureColor * (0.06f + fixture.Level * 0.94f);
        var color = Color.ToSrgb(new Color(glow.X, glow.Y, glow.Z));

        EmitQuad(handle, Texture.White,
            new Vector3(center.X - half.X, center.Y - half.Y, FixtureHeight),
            new Vector3(center.X - half.X, center.Y + half.Y, FixtureHeight),
            new Vector3(center.X + half.X, center.Y + half.Y, FixtureHeight),
            new Vector3(center.X + half.X, center.Y - half.Y, FixtureHeight),
            Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero,
            MaterialEmissive, color);
    }

    private void EmitBillboard(DrawingHandleScreen handle, NullspaceMaze maze, int index)
    {
        var billboard = _frame.Billboards[index];
        var halfSize = NullspaceParams.ClownHeight * 0.5f;
        var center = billboard.Center;
        var right = billboard.Right * halfSize;
        var up = billboard.Up * halfSize;
        var cell = new Vector2i((int) MathF.Floor(center.X / Cell), (int) MathF.Floor(center.Y / Cell));
        var material = billboard.Glows ? MaterialGlowingBillboard : MaterialBillboard;

        EmitQuad(handle, _bakes[index].Texture,
            center - right - up, center + right - up,
            center + right + up, center - right + up,
            new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f),
            Pack(material, Mask(maze, cell)), Color.White);
    }

    private static float Pack(float material, int mask)
    {
        return material + 16f * mask;
    }

    private int Mask(NullspaceMaze maze, Vector2i cell)
    {
        if (_masks.TryGetValue(cell, out var mask))
            return mask;

        mask = 0;
        for (var i = 0; i < _frame.Lamps.Length; i++)
        {
            var lamp = _frame.Lamps[i];
            if (lamp.W > 0f && Lit(maze, lamp.X / Cell, lamp.Y / Cell, cell, LampReach))
                mask |= 1 << i;
        }

        for (var i = 1; i < Math.Min(_frame.Spots.Count, NullspaceFrame.MaxSpots); i++)
        {
            var spot = _frame.Spots[i];
            if (spot.Intensity > 0f && Lit(maze, spot.Position.X / Cell, spot.Position.Y / Cell, cell, SpotReach))
                mask |= 1 << (3 + i);
        }

        _masks[cell] = mask;
        return mask;
    }

    private static bool Lit(NullspaceMaze maze, float x, float y, Vector2i cell, float reach)
    {
        var cx = cell.X + 0.5f;
        var cy = cell.Y + 0.5f;
        var dx = cx - x;
        var dy = cy - y;
        if (dx * dx + dy * dy > reach * reach)
            return true;

        foreach (var sample in VisibilitySamples)
        {
            if (maze.LineClear(x, y, cx + sample.X, cy + sample.Y))
                return true;
        }

        return false;
    }

    private bool CellVisible(Vector2i cell)
    {
        var allNear = true;
        var allLeft = true;
        var allRight = true;
        var allBelow = true;
        var allAbove = true;

        for (var i = 0; i < 8; i++)
        {
            var corner = new Vector3(
                (cell.X + (i & 1)) * Cell,
                (cell.Y + ((i >> 1) & 1)) * Cell,
                (i & 4) == 0 ? WallBottom : WallHeight);

            var rel = corner - _frame.CameraPosition;
            var z = Vector3.Dot(rel, _frame.CameraForward);
            var x = Vector3.Dot(rel, _frame.CameraRight);
            var y = Vector3.Dot(rel, _frame.CameraUp);

            allNear &= z < NearPlane;
            allLeft &= x < -_tanX * z;
            allRight &= x > _tanX * z;
            allBelow &= y < -_tanY * z;
            allAbove &= y > _tanY * z;
        }

        return !(allNear || allLeft || allRight || allBelow || allAbove);
    }

    private void EmitQuad(
        DrawingHandleScreen handle,
        Texture texture,
        Vector3 a,
        Vector3 b,
        Vector3 c,
        Vector3 d,
        Vector2 ta,
        Vector2 tb,
        Vector2 tc,
        Vector2 td,
        float material,
        Color color)
    {
        _clipIn[0] = new ClipVertex(a, ta);
        _clipIn[1] = new ClipVertex(b, tb);
        _clipIn[2] = new ClipVertex(c, tc);
        _clipIn[3] = new ClipVertex(d, td);

        var count = ClipNear(4);
        if (count < 3)
            return;

        if (_batchCount > 0 && _batchTexture != texture)
            Flush(handle);

        _batchTexture = texture;

        var needed = _batchCount + (count - 2) * 3;
        if (_batch.Length < needed)
            Array.Resize(ref _batch, Math.Max(needed, _batch.Length * 2));

        var first = Project(_clipOut[0], material, color);
        var previous = Project(_clipOut[1], material, color);
        for (var i = 2; i < count; i++)
        {
            var current = Project(_clipOut[i], material, color);
            _batch[_batchCount++] = first;
            _batch[_batchCount++] = previous;
            _batch[_batchCount++] = current;
            previous = current;
        }
    }

    private int ClipNear(int count)
    {
        var outCount = 0;
        for (var i = 0; i < count; i++)
        {
            var current = _clipIn[i];
            var next = _clipIn[(i + 1) % count];
            var zCurrent = Depth(current.Position);
            var zNext = Depth(next.Position);
            var inCurrent = zCurrent >= NearPlane;
            var inNext = zNext >= NearPlane;

            if (inCurrent)
                _clipOut[outCount++] = current;

            if (inCurrent == inNext)
                continue;

            var t = (NearPlane - zCurrent) / (zNext - zCurrent);
            _clipOut[outCount++] = new ClipVertex(
                Vector3.Lerp(current.Position, next.Position, t),
                Vector2.Lerp(current.Uv, next.Uv, t));
        }

        return outCount;
    }

    private float Depth(Vector3 position)
    {
        return Vector3.Dot(position - _frame.CameraPosition, _frame.CameraForward);
    }

    private DrawVertexUV2DColor Project(ClipVertex vertex, float material, Color color)
    {
        var rel = vertex.Position - _frame.CameraPosition;
        var z = MathF.Max(Vector3.Dot(rel, _frame.CameraForward), NearPlane);
        var x = Vector3.Dot(rel, _frame.CameraRight);
        var y = Vector3.Dot(rel, _frame.CameraUp);
        var u = 0.5f + _frame.Focal * x / (z * _aspect);
        var v = 0.5f + _frame.Focal * y / z;
        var q = 1f / z;

        return new DrawVertexUV2DColor
        {
            Position = new Vector2(u * _width, (1f - v) * _height),
            UV = vertex.Uv * q,
            UV2 = new Vector2(q, material),
            Color = color,
        };
    }

    private void Flush(DrawingHandleScreen handle)
    {
        if (_batchCount == 0 || _batchTexture == null)
            return;

        handle.DrawPrimitives(
            DrawPrimitiveTopology.TriangleList,
            _batchTexture,
            new ReadOnlySpan<DrawVertexUV2DColor>(_batch, 0, _batchCount));
        _batchCount = 0;
    }

    private void BakeWallFaces(DrawingHandleScreen handle)
    {
        if (!_resources.TryGetResource<RSIResource>(new ResPath(WallRsi), out var resource))
            return;

        var rsi = resource.RSI;
        for (var frame = 0; frame < 4; frame++)
        {
            var west = frame is 0 or 2;
            var east = frame is 0 or 1;
            var box = UIBox2.FromDimensions(new Vector2(frame * WallFrameSize, 0f), new Vector2(WallFrameSize, WallFrameSize));

            DrawCorner(handle, rsi, east ? 4 : 0, RsiDirection.East, box);
            DrawCorner(handle, rsi, east ? 1 : 0, RsiDirection.South, box);
            DrawCorner(handle, rsi, west ? 4 : 0, RsiDirection.West, box);
            DrawCorner(handle, rsi, west ? 1 : 0, RsiDirection.North, box);
        }
    }

    private static void DrawCorner(DrawingHandleScreen handle, RSI rsi, int fill, RsiDirection direction, UIBox2 box)
    {
        if (rsi.TryGetState(WallState + fill, out var state))
            handle.DrawTextureRect(state.GetFrame(direction, 0), box);
    }

    private Texture TileTexture(ITileDefinitionManager tiles, ProtoId<ContentTileDefinition> id, out float uScale)
    {
        uScale = 1f;
        if (tiles[id.Id] is not ContentTileDefinition { Sprite: { } path })
            return Texture.White;

        var texture = _resources.GetResource<TextureResource>(path).Texture;
        uScale = MathF.Min(1f, texture.Height / (float) texture.Width);
        return texture;
    }

    private static Vector3 SrgbToLinear(byte r, byte g, byte b)
    {
        var color = Color.FromSrgb(new Color(r, g, b));
        return new Vector3(color.R, color.G, color.B);
    }

    private readonly record struct ClipVertex(Vector3 Position, Vector2 Uv);
}
