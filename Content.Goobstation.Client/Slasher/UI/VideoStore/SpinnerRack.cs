using System.Numerics;
using System.Runtime.InteropServices;
using Content.Goobstation.Shared.Slasher.UI;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Shared.Input;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Slasher.UI.VideoStore;

/// <summary>
/// Just don't.
/// </summary>
public sealed class SpinnerRack : Control
{
    [Dependency] private readonly IResourceCache _cache = default!;

    public event Action<int, Entry>? OnSelected;

    #region Layout

    private const float TapeWidth = 110f;
    private const float TapeHeight = 174f;
    private const float Radius = 330f;
    private const float FrameRadius = 342f;
    private const float Perspective = 1300f;
    private const int MinSlots = 12;
    private const float SpinSpeed = 7f;
    private const float SlideSpeed = 6f;
    private const float FadeTime = 0.25f;
    private const float BackFace = 0.05f;

    private static readonly float TiltSin = MathF.Sin(MathHelper.DegreesToRadians(6f));
    private static readonly float TiltCos = MathF.Cos(MathHelper.DegreesToRadians(6f));

    private static readonly Vector2 Center = new(400f, 300f);
    private const float EyeAbove = 160f;

    private const float WheelY = 96f;
    private const float CrownY = -100f;
    private const float TierHeight = WheelY - CrownY;
    private const float WheelRadius = 346f;
    private const float HubRadius = 48f;
    private const float PoleWidth = 28f;

    private const float MidHoopY = -20f;
    private const float HoopRadius = 336f;
    private const float HoopFront = 0.45f;
    private const float UprightX = 60f;
    private const float PocketTop = 9f;
    private const float PocketBottom = 89f;
    private const int FanSegments = 48;
    private const float HoopThickness = 6f;

    private const float SignWidth = 460f;
    private const float SignHeight = 96f;
    private const float SignGap = 76f;
    private const float SpokeHalfWidth = 5f;
    private const float CrownRail = 10f;
    private const float FootDrop = 44f;
    private static readonly Vector2 FootSize = new(240f, 70f);

    private const float TagWidth = 30f;
    private const float TagHeight = 18f;

    #endregion

    #region Resources

    private readonly Font _titleFont;
    private readonly Font _bandFont;
    private readonly Font _stickerFont;
    private readonly Texture _body;
    private readonly Texture _overlay;
    private readonly Texture _rated;
    private readonly Texture _pole;
    private readonly Texture _foot;
    private readonly Texture _hub;
    private readonly Texture _wheelRim;
    private readonly Texture _wheelSpokes;
    private readonly Texture _hoopMid;
    private readonly Texture _rodV;
    private readonly Texture _rodH;
    private readonly Texture _mesh;
    private readonly Texture _sign;
    private readonly NeonSign _neon;
    private readonly string _ratedText = Loc.GetString("slasher-kit-store-rated");
    private readonly string _stickerText = Loc.GetString("slasher-kit-store-sticker");

    #endregion

    #region State

    private readonly List<Tier> _tiers = new();
    private int _active;
    private float _slide;

    private readonly List<(int Entry, Vector2[] Quad)> _drawn = new();
    private int _hovered = -1;

    private readonly List<DrawVertexUV2D> _verts = new(FanSegments * 6);
    private readonly Vector2[] _quad = new Vector2[4];
    private readonly List<(int Slot, float Angle, float Facing)> _placed = new();
    private readonly List<(float Facing, Vector3 Foot)> _spokes = new();

    private Matrix3x2 _screen;
    private Matrix3x2 _stage;

    #endregion

    public SpinnerRack()
    {
        IoCManager.InjectDependencies(this);

        _titleFont = StoreFonts.Drip(_cache, 13);
        _bandFont = StoreFonts.Sign(_cache, 12);
        _stickerFont = StoreFonts.Stencil(_cache, 11);
        _body = StoreTextures.Get(_cache, "tape_body");
        _overlay = StoreTextures.Get(_cache, "tape_overlay");
        _rated = StoreTextures.Get(_cache, "tape_r");
        _pole = StoreTextures.Get(_cache, "pole");
        _foot = StoreTextures.Get(_cache, "foot");
        _hub = StoreTextures.Get(_cache, "hub");
        _wheelRim = StoreTextures.Get(_cache, "wheel_rim");
        _wheelSpokes = StoreTextures.Get(_cache, "wheel_spokes");
        _hoopMid = StoreTextures.Get(_cache, "hoop_mid");
        _rodV = StoreTextures.Get(_cache, "rod_v");
        _rodH = StoreTextures.Get(_cache, "rod_h");
        _mesh = StoreTextures.Get(_cache, "mesh");
        _sign = StoreTextures.Get(_cache, "sign");
        _neon = new NeonSign();

        MouseFilter = MouseFilterMode.Stop;
        RectClipContent = true;
        SetSize = new Vector2(780f, 560f);
    }

    #region Public Shit

    public int ActiveTier => _active;
    public int TierCount => _tiers.Count;

    public void SetSign(string text, Color color, string accent, Color accentColor)
    {
        _neon.Set(text, color, accent, accentColor);
    }

    public void SetTier(int at, List<Entry> entries)
    {
        var keep = at < _tiers.Count && HasSameTapes(_tiers[at].Entries, entries);
        var kept = Math.Min(keep ? at + 1 : at, _tiers.Count);
        _tiers.RemoveRange(kept, _tiers.Count - kept);

        if (!keep)
            _tiers.Add(CreateTier(entries));

        _hovered = -1;
        if (_active >= _tiers.Count)
            _active = Math.Max(0, _tiers.Count - 1);
    }

    public void SetActive(int tier)
    {
        if (tier < 0 || tier >= _tiers.Count)
            return;

        _active = tier;
        _hovered = -1;
        var t = _tiers[tier];
        if (t.Selected >= 0)
            OnSelected?.Invoke(tier, t.Entries[t.Selected]);
    }

    public Entry? SelectedOn(int tier)
    {
        if (tier < 0 || tier >= _tiers.Count || _tiers[tier].Selected < 0)
            return null;

        return _tiers[tier].Entries[_tiers[tier].Selected];
    }

    public void Move(int direction)
    {
        if (_active >= _tiers.Count || _tiers[_active].Entries.Count == 0)
            return;

        var tier = _tiers[_active];
        SelectTape((tier.Selected + direction + tier.Entries.Count) % tier.Entries.Count);
    }

    #endregion

    #region Selection

    private static Tier CreateTier(List<Entry> entries)
    {
        var tier = new Tier { Entries = entries };
        if (entries.Count == 0)
            return tier;

        tier.Selected = Math.Max(0, entries.FindIndex(e => e.Kit.Unlocked));
        tier.Rotation = tier.Target = -tier.Selected * tier.Step;
        return tier;
    }

    private static bool HasSameTapes(List<Entry> a, List<Entry> b)
    {
        if (a.Count != b.Count)
            return false;

        for (var i = 0; i < a.Count; i++)
            if (a[i].Index != b[i].Index || a[i].Kit.Unlocked != b[i].Kit.Unlocked)
                return false;

        return true;
    }

    private void SelectTape(int index)
    {
        if (_active >= _tiers.Count)
            return;

        var tier = _tiers[_active];
        if (index < 0 || index >= tier.Entries.Count)
            return;

        tier.Target -= ShortestSlotDelta(tier, index) * tier.Step;
        tier.Selected = index;
        OnSelected?.Invoke(_active, tier.Entries[index]);
    }

    private static int ShortestSlotDelta(Tier tier, int index)
    {
        var delta = index - tier.Selected;
        if (delta < 0)
            delta += tier.Slots;
        if (delta * 2 > tier.Slots)
            delta -= tier.Slots;

        return delta;
    }

    #endregion

    #region Animation

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        var dt = args.DeltaSeconds;
        foreach (var tier in _tiers)
        {
            tier.Fade = MathF.Min(1f, tier.Fade + dt / FadeTime);
            tier.Rotation = EaseToward(tier.Rotation, tier.Target, dt * SpinSpeed);
        }

        _slide = EaseToward(_slide, -_active * TierHeight, dt * SlideSpeed);

        _neon.Update(dt);
    }

    private static float EaseToward(float value, float target, float amount)
    {
        value = MathHelper.Lerp(value, target, MathF.Min(1f, amount));
        return MathF.Abs(value - target) < 0.05f ? target : value;
    }

    #endregion

    #region Input

    protected override void MouseMove(GUIMouseMoveEventArgs args)
    {
        base.MouseMove(args);

        _hovered = TapeAtPoint(args.RelativePosition);
    }

    protected override void MouseExited()
    {
        base.MouseExited();

        _hovered = -1;
    }

    protected override void KeyBindDown(GUIBoundKeyEventArgs args)
    {
        base.KeyBindDown(args);

        if (args.Function != EngineKeyFunctions.UIClick)
            return;

        var hit = TapeAtPoint(args.RelativePosition);
        if (hit < 0)
            return;

        SelectTape(hit);
        args.Handle();
    }

    private int TapeAtPoint(Vector2 point)
    {
        for (var i = _drawn.Count - 1; i >= 0; i--)
            if (IsPointInQuad(point, _drawn[i].Quad))
                return _drawn[i].Entry;

        return -1;
    }

    private static bool IsPointInQuad(Vector2 p, Vector2[] q)
    {
        var sign = 0;
        for (var i = 0; i < 4; i++)
        {
            var a = q[i];
            var b = q[(i + 1) % 4];
            var cross = (b.X - a.X) * (p.Y - a.Y) - (b.Y - a.Y) * (p.X - a.X);
            var side = cross > 0 ? 1 : cross < 0 ? -1 : 0;
            if (side == 0)
                continue;
            if (sign == 0)
                sign = side;
            else if (sign != side)
                return false;
        }

        return sign != 0;
    }

    #endregion

    #region Projection

    private Vector2 ProjectWorldPoint(float x, float y, float z)
    {
        var k = Perspective / (Perspective - z);
        var eyeY = Center.Y - EyeAbove;
        return new Vector2(Center.X + x * k, eyeY + (y + _slide + EyeAbove) * k);
    }

    private Vector2 ProjectWorldPoint(Vector3 point)
    {
        return ProjectWorldPoint(point.X, point.Y, point.Z);
    }

    private Vector2 ProjectRingPoint(int tier, float angle, float lx, float ly, float radius, bool tilted)
    {
        var (sin, cos) = MathF.SinCos(angle);
        var r = tilted ? radius + ly * TiltSin : radius;
        var y = (tilted ? ly * TiltCos : ly) + tier * TierHeight;
        return ProjectWorldPoint(r * sin + lx * cos, y, r * cos - lx * sin);
    }

    private Vector2[] ProjectRingRect(int tier, float angle, UIBox2 rect, float radius = Radius, bool tilted = true)
    {
        _quad[0] = ProjectRingPoint(tier, angle, rect.Left, rect.Top, radius, tilted);
        _quad[1] = ProjectRingPoint(tier, angle, rect.Right, rect.Top, radius, tilted);
        _quad[2] = ProjectRingPoint(tier, angle, rect.Right, rect.Bottom, radius, tilted);
        _quad[3] = ProjectRingPoint(tier, angle, rect.Left, rect.Bottom, radius, tilted);
        return _quad;
    }

    private static Matrix3x2 CreateTapeTextTransform(Vector2[] face, float textScale, float uiScale)
    {
        var xAxis = (face[1] - face[0]) * uiScale / (TapeWidth * textScale);
        var yAxis = (face[3] - face[0]) * uiScale / (TapeHeight * textScale);
        return new Matrix3x2(xAxis.X, xAxis.Y, yAxis.X, yAxis.Y, face[0].X * uiScale, face[0].Y * uiScale);
    }

    private static bool IsOnRingHalf(bool front, float angle)
    {
        return front == MathF.Cos(angle) >= 0f;
    }

    #endregion

    #region Graphics

    private void DrawTexturedQuad(DrawingHandleScreen handle, Texture texture, Vector2[] q, Color color)
    {
        float u0 = 0f, u1 = 1f, vTop = 0f, vBottom = 1f;
        if (texture is AtlasTexture atlas)
        {
            var region = atlas.SubRegion;
            texture = atlas.SourceTexture;
            float w = texture.Width, h = texture.Height;
            u0 = region.Left / w;
            u1 = region.Right / w;
            vTop = (h - region.Top) / h;
            vBottom = (h - region.Bottom) / h;
        }

        _verts.Clear();
        AddQuadVertices(
            new DrawVertexUV2D(q[0], new Vector2(u0, vTop)),
            new DrawVertexUV2D(q[1], new Vector2(u1, vTop)),
            new DrawVertexUV2D(q[2], new Vector2(u1, vBottom)),
            new DrawVertexUV2D(q[3], new Vector2(u0, vBottom)));
        FlushVertices(handle, texture, color);
    }

    private void DrawTextureOnRing(
        DrawingHandleScreen handle,
        Texture texture,
        int tier,
        float angle,
        UIBox2 rect,
        Color color,
        float radius = Radius,
        bool tilted = true)
    {
        DrawTexturedQuad(handle, texture, ProjectRingRect(tier, angle, rect, radius, tilted), color);
    }

    private void DrawDiscHalf(
        DrawingHandleScreen handle,
        Texture texture,
        float radius,
        float y,
        float rotation,
        bool front,
        Color color)
    {
        _verts.Clear();
        var center = new DrawVertexUV2D(ProjectWorldPoint(0f, y, 0f), new Vector2(0.5f, 0.5f));
        var rot = MathHelper.DegreesToRadians(rotation);
        for (var i = 0; i < FanSegments; i++)
        {
            var a0 = i * MathF.Tau / FanSegments;
            var a1 = (i + 1) * MathF.Tau / FanSegments;
            if (!IsOnRingHalf(front, (a0 + a1) * 0.5f + rot))
                continue;

            _verts.Add(center);
            _verts.Add(RimVertex(a0));
            _verts.Add(RimVertex(a1));
        }

        FlushVertices(handle, texture, color);

        DrawVertexUV2D RimVertex(float a)
        {
            var (sin, cos) = MathF.SinCos(a + rot);
            var uv = new Vector2(0.5f + 0.5f * MathF.Sin(a), 0.5f - 0.5f * MathF.Cos(a));
            return new DrawVertexUV2D(ProjectWorldPoint(radius * sin, y, radius * cos), uv);
        }
    }

    private void DrawHoopHalf(
        DrawingHandleScreen handle,
        Texture texture,
        float radius,
        float y,
        float rotation,
        bool front,
        Color color)
    {
        DrawDiscHalf(handle, texture, radius, y + HoopThickness, rotation, front, Darken(color));
        DrawDiscHalf(handle, texture, radius, y, rotation, front, color);
    }

    private void DrawRailBandHalf(
        DrawingHandleScreen handle,
        float radius,
        float top,
        float bottom,
        bool front,
        Color color)
    {
        const float u0 = 0.06f;
        const float u1 = 0.94f;

        _verts.Clear();
        for (var i = 0; i < FanSegments; i++)
        {
            var a0 = i * MathF.Tau / FanSegments;
            var a1 = (i + 1) * MathF.Tau / FanSegments;
            if (!IsOnRingHalf(front, (a0 + a1) * 0.5f))
                continue;

            var (sin0, cos0) = MathF.SinCos(a0);
            var (sin1, cos1) = MathF.SinCos(a1);
            AddQuadVertices(
                new DrawVertexUV2D(ProjectWorldPoint(radius * sin0, top, radius * cos0), new Vector2(u0, 0f)),
                new DrawVertexUV2D(ProjectWorldPoint(radius * sin1, top, radius * cos1), new Vector2(u1, 0f)),
                new DrawVertexUV2D(ProjectWorldPoint(radius * sin1, bottom, radius * cos1), new Vector2(u1, 1f)),
                new DrawVertexUV2D(ProjectWorldPoint(radius * sin0, bottom, radius * cos0), new Vector2(u0, 1f)));
        }

        FlushVertices(handle, _rodH, color);
    }

    private void DrawRod(DrawingHandleScreen handle, Vector3 from, Vector3 to, Color color)
    {
        var a = ProjectWorldPoint(from);
        var b = ProjectWorldPoint(to);
        var dir = b - a;
        if (dir.LengthSquared() < 1f)
            return;

        dir = Vector2.Normalize(dir);
        var n = new Vector2(-dir.Y, dir.X) * SpokeHalfWidth;
        _quad[0] = a - n;
        _quad[1] = a + n;
        _quad[2] = b + n;
        _quad[3] = b - n;
        DrawTexturedQuad(handle, _rodV, _quad, color);
    }

    private void AddQuadVertices(DrawVertexUV2D tl, DrawVertexUV2D tr, DrawVertexUV2D br, DrawVertexUV2D bl)
    {
        _verts.Add(tl);
        _verts.Add(tr);
        _verts.Add(br);
        _verts.Add(tl);
        _verts.Add(br);
        _verts.Add(bl);
    }

    private void FlushVertices(DrawingHandleScreen handle, Texture texture, Color color)
    {
        if (_verts.Count == 0)
            return;

        var verts = CollectionsMarshal.AsSpan(_verts);
        handle.DrawPrimitives(DrawPrimitiveTopology.TriangleList, texture, verts, color);
    }

    private static void DrawTextCentered(
        DrawingHandleScreen handle,
        Font font,
        string text,
        float top,
        float scale,
        Color color,
        bool fit,
        float left = 0f,
        float width = TapeWidth,
        float reserve = 0f)
    {
        var textScale = scale;
        var measured = StoreFonts.MeasureWidth(font, text, textScale);
        var room = (width - 8f - reserve * 2f) * scale;
        if (fit && measured > room && measured > 0f)
        {
            textScale *= room / measured;
            measured = room;
        }

        var pos = new Vector2(left * scale + (width * scale - measured) * 0.5f, top * scale);
        if (fit)
            handle.DrawString(font, pos + new Vector2(1.5f) * scale, text, textScale, new Color(0f, 0f, 0f, color.A));
        handle.DrawString(font, pos, text, textScale, color);
    }

    #endregion

    #region Colors

    private static float BrightnessForFacing(float facing)
    {
        return 0.35f + 0.65f * (facing + 1f) * 0.5f;
    }

    private static Color Grey(float brightness, float alpha = 1f)
    {
        return new Color(brightness, brightness, brightness, alpha);
    }

    private static Color Darken(Color color)
    {
        return new Color(color.R * 0.45f, color.G * 0.45f, color.B * 0.45f, color.A);
    }

    private static Color DesaturateLockedSpine(Color color)
    {
        var grey = 0.299f * color.R + 0.587f * color.G + 0.114f * color.B;
        return new Color(
            MathHelper.Lerp(color.R, grey, 0.8f) * 0.45f,
            MathHelper.Lerp(color.G, grey, 0.8f) * 0.45f,
            MathHelper.Lerp(color.B, grey, 0.8f) * 0.45f);
    }

    #endregion

    #region Rack

    protected override void Draw(DrawingHandleScreen handle)
    {
        _drawn.Clear();
        if (_tiers.Count == 0)
            return;

        _screen = handle.GetTransform();
        _stage = Matrix3Helpers.CreateScale(new Vector2(UIScale)) * _screen;
        handle.SetTransform(_stage);

        DrawRackFoot(handle);
        for (var t = _tiers.Count - 1; t >= 0; t--)
            DrawTier(handle, t);
        DrawSignBoard(handle);

        handle.SetTransform(_screen);
    }

    private void DrawTier(DrawingHandleScreen handle, int t)
    {
        var tier = _tiers[t];
        var dim = t == _active ? 1f : 0.55f;
        var wheelY = WheelY + t * TierHeight;
        var hoopY = MidHoopY + t * TierHeight;

        _placed.Clear();
        for (var i = 0; i < tier.Slots; i++)
        {
            var angle = MathHelper.DegreesToRadians(i * tier.Step + tier.Rotation);
            _placed.Add((i, angle, MathF.Cos(angle)));
        }

        _placed.Sort(static (a, b) => a.Facing.CompareTo(b.Facing));

        var far = Grey(0.55f * dim);
        var near = Grey(dim);
        DrawHoopHalf(handle, _wheelRim, WheelRadius, wheelY, tier.Rotation, false, far);
        DrawDiscHalf(handle, _wheelSpokes, WheelRadius, wheelY, tier.Rotation, false, far);
        DrawHoopHalf(handle, _hoopMid, HoopRadius, hoopY, tier.Rotation, false, far);
        if (t == 0)
            DrawCrownHalf(handle, false, far);

        var wheelDrawn = false;
        var hoopDrawn = false;
        foreach (var (slot, angle, facing) in _placed)
        {
            if (!wheelDrawn && facing >= 0f)
            {
                DrawWheelFrontAndPole(handle, t, near, far);
                wheelDrawn = true;
            }

            if (!hoopDrawn && facing >= HoopFront)
            {
                DrawHoopHalf(handle, _hoopMid, HoopRadius, hoopY, tier.Rotation, true, near);
                hoopDrawn = true;
            }

            var brightness = BrightnessForFacing(facing) * dim;
            if (slot < tier.Entries.Count && facing > BackFace)
                DrawTape(handle, t, slot, angle, brightness);

            if (facing > -0.15f)
                DrawSlotFrame(handle, t, angle, brightness);
        }

        if (t == 0)
            DrawCrownHalf(handle, true, near);
    }

    private void DrawWheelFrontAndPole(DrawingHandleScreen handle, int t, Color near, Color far)
    {
        var tier = _tiers[t];
        var wheelY = WheelY + t * TierHeight;
        DrawDiscHalf(handle, _wheelSpokes, WheelRadius, wheelY, tier.Rotation, true, near);
        DrawDiscHalf(handle, _hub, HubRadius, wheelY, 0f, false, far);
        DrawTierPole(handle, t);
        DrawDiscHalf(handle, _hub, HubRadius, wheelY, 0f, true, near);
        DrawHoopHalf(handle, _wheelRim, WheelRadius, wheelY, tier.Rotation, true, near);
    }

    private void DrawCrownHalf(DrawingHandleScreen handle, bool front, Color color)
    {
        DrawRailBandHalf(handle, FrameRadius, CrownY, CrownY + HoopThickness, front, Darken(color));
        DrawRailBandHalf(handle, FrameRadius, CrownY - CrownRail, CrownY, front, color);
    }

    private void DrawTierPole(DrawingHandleScreen handle, int t)
    {
        var yOffset = t * TierHeight;
        var isBottom = t == _tiers.Count - 1;
        var top = ProjectWorldPoint(0f, CrownY + yOffset, 0f);
        var bottom = ProjectWorldPoint(0f, WheelY + yOffset + (isBottom ? FootDrop : 0f), 0f);
        handle.DrawTextureRect(_pole, new UIBox2(top.X - PoleWidth * 0.5f, top.Y, top.X + PoleWidth * 0.5f, bottom.Y));
    }

    private void DrawRackFoot(DrawingHandleScreen handle)
    {
        var center = ProjectWorldPoint(0f, WheelY + (_tiers.Count - 1) * TierHeight + FootDrop, 0f);
        var topLeft = center - new Vector2(FootSize.X * 0.5f, FootSize.Y * 0.6f);
        handle.DrawTextureRect(_foot, UIBox2.FromDimensions(topLeft, FootSize));
    }

    private void DrawSlotFrame(DrawingHandleScreen handle, int t, float angle, float brightness)
    {
        const float half = UprightX + 6f;
        var shade = Grey(brightness);
        DrawFramePart(_mesh, new UIBox2(-half + 6f, PocketTop + 8f, half - 6f, PocketBottom - 9f));
        DrawFramePart(_rodH, new UIBox2(-half, PocketTop, half, PocketTop + 8f));
        DrawFramePart(_rodH, new UIBox2(-half, PocketBottom - 9f, half, PocketBottom));
        DrawFramePart(_rodV, new UIBox2(-UprightX - 6f, CrownY, -UprightX + 6f, WheelY + 4f));
        DrawFramePart(_rodV, new UIBox2(UprightX - 6f, CrownY, UprightX + 6f, WheelY + 4f));

        void DrawFramePart(Texture texture, UIBox2 rect)
        {
            DrawTextureOnRing(handle, texture, t, angle, rect, shade, FrameRadius, false);
        }
    }

    #endregion

    #region Sign

    private void DrawSignBoard(DrawingHandleScreen handle)
    {
        const float top = CrownY - SignGap - SignHeight;
        const float bottom = CrownY - SignGap;

        CollectSignSpokes();
        var apex = new Vector3(0f, bottom + 2f, 0f);
        DrawSignSpokes(handle, apex, false);
        DrawSignPole(handle, (top + bottom) * 0.5f);
        DrawSignSpokes(handle, apex, true);

        var board = ProjectRingRect(0, 0f, new UIBox2(-SignWidth * 0.5f, top, SignWidth * 0.5f, bottom), 0f, false);
        DrawTexturedQuad(handle, _sign, board, Color.White);

        var center = (board[0] + board[2]) * 0.5f;
        handle.SetTransform(_screen);
        _neon.DrawAt(handle, center * UIScale, UIScale * 0.95f);
        handle.SetTransform(_stage);
    }

    private void CollectSignSpokes()
    {
        const float footRadius = FrameRadius - 10f;
        var tier = _tiers[0];

        _spokes.Clear();
        for (var i = 0; i < tier.Slots; i++)
        {
            var (sin, cos) = MathF.SinCos(MathHelper.DegreesToRadians(i * tier.Step + tier.Rotation));
            _spokes.Add((cos, new Vector3(footRadius * sin, CrownY - CrownRail + 2f, footRadius * cos)));
        }

        _spokes.Sort(static (a, b) => a.Facing.CompareTo(b.Facing));
    }

    private void DrawSignSpokes(DrawingHandleScreen handle, Vector3 apex, bool front)
    {
        foreach (var (facing, foot) in _spokes)
            if (facing >= 0f == front)
                DrawRod(handle, apex, foot, Grey(BrightnessForFacing(facing)));
    }

    private void DrawSignPole(DrawingHandleScreen handle, float boardMidY)
    {
        var poleFoot = ProjectWorldPoint(0f, CrownY, 0f);
        var boardMid = ProjectWorldPoint(0f, boardMidY, 0f);
        var pole = new UIBox2(poleFoot.X - PoleWidth * 0.5f, boardMid.Y, poleFoot.X + PoleWidth * 0.5f, poleFoot.Y);
        handle.DrawTextureRect(_pole, pole);
    }

    #endregion

    #region Tapes

    private void DrawTape(DrawingHandleScreen handle, int t, int slot, float angle, float brightness)
    {
        const float hw = TapeWidth * 0.5f;
        const float hh = TapeHeight * 0.5f;

        var tier = _tiers[t];
        var active = t == _active;
        var selected = active && slot == tier.Selected;
        var hovered = active && slot == _hovered;
        var alpha = tier.Fade;

        var entry = tier.Entries[slot];
        var locked = !entry.Kit.Unlocked;
        var tint = locked && !selected ? DesaturateLockedSpine(entry.Spine) : entry.Spine;
        if (hovered && !selected)
            brightness = MathF.Min(1f, brightness + 0.15f);

        var shade = Grey(brightness, alpha);
        var ink = StorePalette.Ink.WithAlpha(alpha);
        var face = (Vector2[]) ProjectRingRect(t, angle, new UIBox2(-hw, -hh, hw, hh)).Clone();

        DrawTexturedQuad(handle, _body, face, tint * shade);
        DrawTexturedQuad(handle, _overlay, face, shade);
        DrawTextureOnRing(handle, entry.Icon, t, angle, new UIBox2(-44f, -34f, 44f, 54f), shade);
        DrawTextureOnRing(handle, _rated, t, angle, new UIBox2(-hw + 4f, hh - 20f, -hw + 20f, hh - 4f), shade);

        var perspectiveScale = Perspective / (Perspective - Radius * MathF.Cos(angle));
        var textScale = perspectiveScale * UIScale;
        handle.SetTransform(CreateTapeTextTransform(face, textScale, UIScale) * _screen);

        var titleTop = 8f;
        if (entry.Band != null)
        {
            DrawPartBand(handle, entry.Band, textScale, shade, ink);
            titleTop = 22f;
        }

        var hasTag = entry.Sequels > 0;
        var reserve = hasTag ? TagWidth + 6f : 0f;
        var title = entry.Name.ToUpperInvariant();
        DrawTextCentered(handle, _titleFont, title, titleTop, textScale, shade, true, 0f, TapeWidth, reserve);
        DrawTextCentered(handle, _bandFont, _ratedText, TapeHeight - 20f, textScale, ink, false, 4f, 16f);

        if (hasTag)
            DrawSequelCountTag(handle, entry.Sequels, titleTop - 4f, textScale, shade, ink);

        if (locked)
            DrawOutOfStockSticker(handle, textScale, shade, alpha);

        handle.SetTransform(_stage);
        if (active)
            _drawn.Add((slot, face));
    }

    private void DrawPartBand(DrawingHandleScreen handle, string text, float textScale, Color shade, Color ink)
    {
        var band = UIBox2.FromDimensions(new Vector2(3f) * textScale, new Vector2(TapeWidth - 6f, 16f) * textScale);
        handle.DrawRect(band, StorePalette.Yellow * shade);
        DrawTextCentered(handle, _bandFont, text, 4f, textScale, ink, false);
    }

    private void DrawSequelCountTag(
        DrawingHandleScreen handle,
        int sequels,
        float tagTop,
        float textScale,
        Color shade,
        Color ink)
    {
        const float tagLeft = TapeWidth - 4f - TagWidth;

        var tagSize = new Vector2(TagWidth, TagHeight) * textScale;
        var tag = UIBox2.FromDimensions(new Vector2(tagLeft, tagTop) * textScale, tagSize);
        var border = new Vector2(1.5f) * textScale;
        var inner = UIBox2.FromDimensions(tag.TopLeft + border, tagSize - border * 2f);
        handle.DrawRect(tag, StorePalette.Ink * shade);
        handle.DrawRect(inner, StorePalette.Yellow * shade);

        var textTop = tagTop + (TagHeight - _bandFont.GetHeight(1f)) * 0.5f + 1f;
        DrawTextCentered(handle, _bandFont, "+" + sequels, textTop, textScale, ink, false, tagLeft, TagWidth);
    }

    private void DrawOutOfStockSticker(DrawingHandleScreen handle, float textScale, Color shade, float alpha)
    {
        const float hw = TapeWidth * 0.5f;
        const float hh = TapeHeight * 0.5f;

        var tape = handle.GetTransform();
        var tilt = new Angle(MathHelper.DegreesToRadians(-6f));
        handle.SetTransform(Matrix3Helpers.CreateTransform(new Vector2(hw, hh - 18f) * textScale, tilt) * tape);

        var bandSize = new Vector2(TapeWidth - 10f, 22f) * textScale;
        var bandTop = new Vector2(-bandSize.X * 0.5f, -bandSize.Y * 0.5f);
        var shadow = UIBox2.FromDimensions(bandTop + new Vector2(0f, 3f) * textScale, bandSize);
        handle.DrawRect(shadow, new Color(0f, 0f, 0f, 0.5f * alpha));
        handle.DrawRect(UIBox2.FromDimensions(bandTop, bandSize), StorePalette.Red * shade);

        var stickerScale = textScale;
        var width = StoreFonts.MeasureWidth(_stickerFont, _stickerText, stickerScale);
        var room = bandSize.X - 10f * textScale;
        if (width > room && width > 0f)
        {
            stickerScale *= room / width;
            width = room;
        }

        var pos = new Vector2(-width * 0.5f, -_stickerFont.GetLineHeight(stickerScale) * 0.5f);
        handle.DrawString(_stickerFont, pos, _stickerText, stickerScale, shade);
        handle.SetTransform(tape);
    }

    #endregion

    #region Types

    public sealed record Entry(
        SlasherKitInfo Kit,
        int Index,
        string Name,
        Texture Icon,
        Color Spine,
        int Sequels,
        int Depth)
    {
        public string? Band { get; } = Depth > 0 ? StoreFonts.Part(Depth + 1) : null;
    }

    private sealed class Tier
    {
        public List<Entry> Entries = new();
        public int Selected = -1;
        public float Rotation;
        public float Target;
        public float Fade;

        public int Slots => Math.Max(Entries.Count, MinSlots);
        public float Step => 360f / Slots;
    }

    #endregion
}
