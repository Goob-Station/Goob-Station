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

    private readonly Texture _panel;
    private readonly Texture _under;
    private readonly Texture _bolt;
    private readonly Vector2[] _boltPositions;
    private readonly int[] _turns = new int[4];
    private readonly TimeSpan[] _lastTurn = new TimeSpan[4];
    private readonly TimeSpan?[] _detached = new TimeSpan?[4];

    private TimeSpan? _fallStart;
    private TimeSpan? _landedAt;
    private float _groundOffset;

    public bool WrenchHeld { get; set; }

    public event Action? Landed;

    public MetalPanelHatch()
    {
        IoCManager.InjectDependencies(this);
        MouseFilter = MouseFilterMode.Stop;

        _panel = Load("metal_panel");
        _under = Load("under_metal_panel");
        _bolt = Load("bolt");

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

    public void Reset()
    {
        Array.Clear(_turns);
        Array.Clear(_lastTurn);
        Array.Clear(_detached);
        _fallStart = null;
        _landedAt = null;
        _groundOffset = 0;
        WrenchHeld = false;
    }

    protected override Vector2 MeasureOverride(Vector2 availableSize)
    {
        return (Vector2) _panel.Size;
    }

    private void OnClicked(GUIBoundKeyEventArgs args)
    {
        if (args.Function != EngineKeyFunctions.UIClick || !WrenchHeld || Fallen)
            return;

        var position = args.RelativePixelPosition / UIScale;
        var now = _timing.RealTime;
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

        var now = _timing.RealTime;
        var offset = 0f;
        var angle = 0f;
        var alpha = 1f;

        if (_fallStart is { } start && now >= start)
        {
            offset = _landedAt != null ? _groundOffset : FallDistance(start);
            angle = MathF.Min((float) (now - start).TotalSeconds * 0.9f, 0.18f);

            if (_landedAt is { } landed)
            {
                var resting = (float) (now - landed).TotalSeconds - RestSeconds;
                alpha = 1f - Math.Clamp(resting / FadeSeconds, 0f, 1f);
            }
        }

        if (alpha > 0)
            DrawPanel(handle, now, scale, offset, angle, alpha);

        DrawFallingBolts(handle, now, scale);
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

    private Texture Load(string name)
    {
        return _cache.GetResource<TextureResource>($"{TexturePath}/{name}.png").Texture;
    }
}
