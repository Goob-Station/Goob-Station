using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Input;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Goobstation.UIKit.UserInterface.Lobby;

public sealed partial class MetalPanelHatch : Control
{
    [Dependency] private IResourceCache _cache = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IEntityManager _entMan = default!;

    private const string TexturePath = "/Textures/_Goobstation/Interface/Lobby";
    private const float TurnSeconds = 0.12f;
    private const int TurnsToRemove = 3;
    private const float FallDelaySeconds = 0.35f;
    private const float Gravity = 2200f;
    private const float RestSeconds = 1.5f;
    private const float FadeSeconds = 0.6f;

    private static readonly SoundSpecifier RatchetSound = new SoundPathSpecifier("/Audio/Items/ratchet.ogg");
    private static readonly SoundSpecifier LandSound = new SoundPathSpecifier("/Audio/Effects/metal_slam2.ogg");
    private static readonly SoundSpecifier PeelSound = new SoundPathSpecifier("/Audio/_Goobstation/Items/handling/paper_pickup.ogg");
    private static readonly SoundSpecifier TearSound = new SoundPathSpecifier("/Audio/Effects/unwrap.ogg");
    private static readonly SoundSpecifier WindSound = new SoundPathSpecifier("/Audio/Effects/space_wind.ogg");

    private const string NotePath = "/Textures/_Goobstation/Interface/Lobby/Note";
    private const float PeelFrameSeconds = 0.07f;
    private const float TornHoldSeconds = 0.75f;
    private const float WindAcceleration = 1500f;

    private static readonly UIBox2 NoteCorner = new(133, 141, 182, 169);
    private static readonly Vector2[] PieceOffsets =
    {
        new(20, 1), new(35, 2), new(55, 10), new(9, 26), new(18, 34),
        new(55, 44), new(77, 45), new(11, 63), new(37, 64), new(71, 67),
    };

    private readonly Texture _panel;
    private readonly Texture _under;
    private readonly Texture _note;
    private readonly Texture _noteHover;
    private readonly Texture _tinyNote;
    private readonly Texture[] _peel;
    private readonly Texture[] _pieces;
    private readonly Vector2 _canvasOffset;
    private readonly Texture _bolt;
    private readonly Vector2[] _boltPositions;
    private readonly int[] _turns = new int[4];
    private readonly TimeSpan[] _lastTurn = new TimeSpan[4];
    private readonly TimeSpan?[] _detached = new TimeSpan?[4];

    private TimeSpan? _fallStart;
    private TimeSpan? _landedAt;
    private float _groundOffset;
    private TimeSpan? _tornAt;
    private bool _tearPlayed;
    private bool _windPlayed;
    private bool _noteHovered;

    public bool WrenchHeld { get; set; }

    public event Action? Landed;

    public MetalPanelHatch()
    {
        IoCManager.InjectDependencies(this);
        MouseFilter = MouseFilterMode.Stop;

        _panel = Load("metal_panel");
        _under = Load("frame_under_panel");
        _bolt = Load("bolt");
        _note = LoadNote("note");
        _noteHover = LoadNote("note_hover");
        _tinyNote = LoadNote("tiny_note");
        _peel = new[] { LoadNote("note_anim1"), LoadNote("note_anim2"), LoadNote("note_anim3"), LoadNote("note_anim4") };
        _pieces = new Texture[PieceOffsets.Length];
        for (var i = 0; i < _pieces.Length; i++)
        {
            _pieces[i] = LoadNote($"note_piece_{i}");
        }

        _canvasOffset = (Vector2) _panel.Size - (Vector2) _note.Size;

        var size = (Vector2) _panel.Size;
        var bolt = (Vector2) _bolt.Size;
        _boltPositions = new[]
        {
            new Vector2(1, 1),
            new Vector2(size.X - bolt.X - 1, 1),
            new Vector2(1, size.Y - bolt.Y - 1),
            new Vector2(size.X - bolt.X - 1, size.Y - bolt.Y - 1),
        };

        OnKeyBindDown += OnClicked;
    }

    private bool Fallen => _fallStart != null;

    private bool NoteInteractive => _landedAt != null && _tornAt == null;

    public void Reset()
    {
        Array.Clear(_turns);
        Array.Clear(_lastTurn);
        Array.Clear(_detached);
        _fallStart = null;
        _landedAt = null;
        _groundOffset = 0;
        _tornAt = null;
        _tearPlayed = false;
        _windPlayed = false;
        _noteHovered = false;
        WrenchHeld = false;
    }

    protected override Vector2 MeasureOverride(Vector2 availableSize)
    {
        return (Vector2) _panel.Size;
    }

    private void OnClicked(GUIBoundKeyEventArgs args)
    {
        if (args.Function != EngineKeyFunctions.UIClick)
            return;

        var position = args.RelativePixelPosition / UIScale;
        var now = _timing.RealTime;

        if (NoteInteractive && OverNote(position))
        {
            _tornAt = now;
            _noteHovered = false;
            _entMan.System<SharedAudioSystem>().PlayGlobal(PeelSound, Filter.Local(), false);
            args.Handle();
            return;
        }

        if (!WrenchHeld || Fallen)
            return;

        for (var i = 0; i < _boltPositions.Length; i++)
        {
            if (_turns[i] >= TurnsToRemove)
                continue;

            if (!UIBox2.FromDimensions(_boltPositions[i], (Vector2) _bolt.Size).Contains(position))
                continue;

            _turns[i]++;
            _lastTurn[i] = now;
            _entMan.System<SharedAudioSystem>().PlayGlobal(RatchetSound, Filter.Local(), false);
            args.Handle();

            if (_turns[i] < TurnsToRemove)
                return;

            _detached[i] = now + TimeSpan.FromSeconds(TurnSeconds);
            if (AllBoltsRemoved())
                _fallStart = now + TimeSpan.FromSeconds(TurnSeconds + FallDelaySeconds);

            return;
        }
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        _noteHovered = NoteInteractive && OverNote((UserInterfaceManager.MousePositionScaled.Position - GlobalPosition));

        if (_tornAt is { } peeled && !_tearPlayed && _timing.RealTime >= peeled + TimeSpan.FromSeconds(PeelFrameSeconds * _peel.Length))
        {
            _tearPlayed = true;
            _entMan.System<SharedAudioSystem>().PlayGlobal(TearSound, Filter.Local(), false);
        }

        if (_tornAt is { } torn && !_windPlayed && _timing.RealTime >= torn + TimeSpan.FromSeconds(WindStartSeconds))
        {
            _windPlayed = true;
            _entMan.System<SharedAudioSystem>().PlayGlobal(WindSound, Filter.Local(), false);
        }

        if (_fallStart is not { } start || _landedAt != null || _timing.RealTime < start)
            return;

        var ground = UserInterfaceManager.RootControl.PixelSize.Y - GlobalPixelPosition.Y - PixelSize.Y;
        if (FallDistance(start) < ground)
            return;

        _groundOffset = ground;
        _landedAt = _timing.RealTime;
        _entMan.System<SharedAudioSystem>().PlayGlobal(LandSound, Filter.Local(), false);
        Landed?.Invoke();
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        base.Draw(handle);

        var scale = UIScale;
        handle.DrawTextureRect(_under, UIBox2.FromDimensions(Vector2.Zero, (Vector2) _under.Size * scale));
        DrawCanvas(handle, _tinyNote, scale);

        if (_tornAt == null)
            DrawCanvas(handle, _noteHovered ? _noteHover : _note, scale);

        var now = _timing.RealTime;
        if (_fallStart is not { } start || now < start)
            DrawPanel(handle, now, scale, 0f, 0f, 1f);
    }

    public void DrawOnTop(DrawingHandleScreen handle, Vector2 pixelOffset)
    {
        var scale = UIScale;
        var now = _timing.RealTime;
        var offset = 0f;
        var angle = 0f;
        var alpha = 1f;

        var baseTransform = handle.GetTransform();
        handle.SetTransform(Matrix3x2.CreateTranslation(pixelOffset) * baseTransform);

        DrawTornNote(handle, now, scale);

        if (_fallStart is { } start && now >= start)
        {
            offset = _landedAt != null ? _groundOffset : FallDistance(start);
            angle = MathF.Min((float) (now - start).TotalSeconds * 0.9f, 0.18f);

            if (_landedAt is { } landed)
            {
                var resting = (float) (now - landed).TotalSeconds - RestSeconds;
                alpha = 1f - Math.Clamp(resting / FadeSeconds, 0f, 1f);
            }

            if (alpha > 0)
                DrawPanel(handle, now, scale, offset, angle, alpha);
        }

        DrawFallingBolts(handle, now, scale);
        handle.SetTransform(baseTransform);
    }

    private void DrawPanel(DrawingHandleScreen handle, TimeSpan now, float scale, float offset, float angle, float alpha)
    {
        var baseTransform = handle.GetTransform();
        var center = (Vector2) _panel.Size * scale / 2f;
        var transform = Matrix3x2.CreateRotation(angle, center) * Matrix3x2.CreateTranslation(0, offset);
        handle.SetTransform(transform * baseTransform);

        var tint = Color.White.WithAlpha(alpha);
        handle.DrawTextureRect(_panel, UIBox2.FromDimensions(Vector2.Zero, (Vector2) _panel.Size * scale), tint);

        for (var i = 0; i < _boltPositions.Length; i++)
        {
            if (_detached[i] is not { } detached || now < detached)
                DrawBolt(handle, i, now, scale, alpha, transform * baseTransform);
        }

        handle.SetTransform(baseTransform);
    }

    private void DrawFallingBolts(DrawingHandleScreen handle, TimeSpan now, float scale)
    {
        var baseTransform = handle.GetTransform();
        var bottom = UserInterfaceManager.RootControl.PixelSize.Y - GlobalPixelPosition.Y;
        var size = (Vector2) _bolt.Size * scale;

        for (var i = 0; i < _boltPositions.Length; i++)
        {
            if (_detached[i] is not { } detached || now < detached)
                continue;

            var seconds = (float) (now - detached).TotalSeconds;
            var position = _boltPositions[i] * scale + new Vector2(0, 0.5f * Gravity * scale * seconds * seconds - TurnsToRemove * scale);
            if (position.Y > bottom)
                continue;

            var spin = Matrix3x2.CreateRotation(MathF.PI / 2f * TurnsToRemove + seconds * 9f, position + size / 2f);
            handle.SetTransform(spin * baseTransform);
            handle.DrawTextureRect(_bolt, UIBox2.FromDimensions(position, size));
        }

        handle.SetTransform(baseTransform);
    }

    private void DrawBolt(DrawingHandleScreen handle, int index, TimeSpan now, float scale, float alpha, Matrix3x2 parent)
    {
        var turned = (float) _turns[index];
        if (_turns[index] > 0)
        {
            var progress = Math.Clamp((float) (now - _lastTurn[index]).TotalSeconds / TurnSeconds, 0f, 1f);
            turned = _turns[index] - 1 + (1f - (1f - progress) * (1f - progress));
        }

        var size = (Vector2) _bolt.Size * scale;
        var position = _boltPositions[index] * scale - new Vector2(0, turned * scale);
        var spin = Matrix3x2.CreateRotation(turned * MathF.PI / 2f, position + size / 2f);

        handle.SetTransform(spin * parent);
        handle.DrawTextureRect(_bolt, UIBox2.FromDimensions(position, size), Color.White.WithAlpha(alpha));
        handle.SetTransform(parent);
    }

    private float WindStartSeconds => PeelFrameSeconds * _peel.Length + TornHoldSeconds;

    private bool OverNote(Vector2 position)
    {
        return NoteCorner.Contains(position - _canvasOffset);
    }

    private void DrawCanvas(DrawingHandleScreen handle, Texture texture, float scale)
    {
        handle.DrawTextureRect(texture, UIBox2.FromDimensions(_canvasOffset * scale, (Vector2) texture.Size * scale));
    }

    private void DrawTornNote(DrawingHandleScreen handle, TimeSpan now, float scale)
    {
        if (_tornAt is not { } torn)
            return;

        var seconds = (float) (now - torn).TotalSeconds;
        var frame = (int) (seconds / PeelFrameSeconds);
        if (frame < _peel.Length)
        {
            DrawCanvas(handle, _peel[frame], scale);
            return;
        }

        var baseTransform = handle.GetTransform();
        var right = UserInterfaceManager.RootControl.PixelSize.X - GlobalPixelPosition.X;
        var wind = seconds - WindStartSeconds;

        for (var i = 0; i < _pieces.Length; i++)
        {
            var texture = _pieces[i];
            var size = (Vector2) texture.Size * scale;
            var position = (_canvasOffset + PieceOffsets[i]) * scale;
            var angle = 0f;

            var t = wind - Noise(i, 1) * 0.25f;
            if (t > 0)
            {
                var push = WindAcceleration * (0.7f + Noise(i, 2) * 0.6f);
                position += new Vector2(
                    0.5f * push * t * t,
                    -(30f + Noise(i, 3) * 50f) * t + MathF.Sin(t * (5f + Noise(i, 4) * 4f) + i) * 8f) * scale;
                angle = t * (Noise(i, 5) * 8f - 4f);
            }

            if (position.X > right)
                continue;

            handle.SetTransform(Matrix3x2.CreateRotation(angle, position + size / 2f) * baseTransform);
            handle.DrawTextureRect(texture, UIBox2.FromDimensions(position, size));
        }

        handle.SetTransform(baseTransform);
    }

    private static float Noise(int index, int salt)
    {
        var value = MathF.Sin(index * 12.9898f + salt * 78.233f) * 43758.547f;
        return value - MathF.Floor(value);
    }

    private bool AllBoltsRemoved()
    {
        foreach (var detached in _detached)
        {
            if (detached == null)
                return false;
        }

        return true;
    }

    private float FallDistance(TimeSpan start)
    {
        var seconds = (float) (_timing.RealTime - start).TotalSeconds;
        return 0.5f * Gravity * UIScale * seconds * seconds;
    }

    private Texture LoadNote(string name)
    {
        return _cache.GetResource<TextureResource>($"{NotePath}/{name}.png").Texture;
    }

    private Texture Load(string name)
    {
        return _cache.GetResource<TextureResource>($"{TexturePath}/{name}.png").Texture;
    }
}
