using System.Numerics;
using Content.Client.Lobby;
using Robust.Client;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Console;
using Robust.Shared.Graphics.RSI;
using Robust.Shared.Network;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.MisandryBox.Spider;

public sealed class SpiderUIController : UIController
{
    [Dependency] private readonly IRobustRandom _random = default!;

    private SpiderWidget? _spider;
    private Vector2 _position;
    private Vector2 _direction;
    private float _timeUntilDirectionChange;
    private bool _enabled;
    private Vector2 _lastScreenSize;
    public bool Permanent;

    private const float Speed = 120f;
    private const float DirectionChangeInterval = 4f;

    public void Toggle()
    {
        SetEnabled(!_enabled);
    }

    public void SetEnabled(bool enabled)
    {
        _enabled = enabled;

        if (!ShouldShowSpider() && _spider != null)
        {
            _spider.Parent?.RemoveChild(_spider);
            _spider = null;
        }
        else if (ShouldShowSpider() && _spider == null)
        {
            var screen = UIManager.ActiveScreen;
            if (screen != null)
            {
                InitializeSpider(screen);
            }
        }
    }

    private bool ShouldShowSpider() => _enabled || Permanent;

    public override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        if (!ShouldShowSpider())
            return;

        var screen = UIManager.ActiveScreen;
        if (screen == null)
            return;

        if (_spider == null)
        {
            InitializeSpider(screen);
            return;
        }

        UpdateSpider(screen, args.DeltaSeconds);
    }

    private void InitializeSpider(UIScreen screen)
    {
        _spider = new SpiderWidget();

        UIManager.WindowRoot.Root?.AddChild(_spider);

        _spider.Measure(new Vector2(float.PositiveInfinity));
        _spider.InvalidateArrange();
        LayoutContainer.SetAnchorPreset(_spider, LayoutContainer.LayoutPreset.Center);
        LayoutContainer.SetPosition(_spider, _position);

        _lastScreenSize = screen.Size;
        _position = new Vector2(
            _random.NextFloat() * screen.Size.X,
            _random.NextFloat() * screen.Size.Y
        );

        _direction = GetRandomDirection();
        _timeUntilDirectionChange = DirectionChangeInterval;
    }

    private void UpdateSpider(UIScreen screen, float deltaTime)
    {
        if (_spider == null)
            return;

        var currentScreenSize = screen.Size;

        if (_lastScreenSize != currentScreenSize)
        {
            _position.X = Math.Clamp(_position.X, 0, currentScreenSize.X);
            _position.Y = Math.Clamp(_position.Y, 0, currentScreenSize.Y);
            _lastScreenSize = currentScreenSize;
        }

        _timeUntilDirectionChange -= deltaTime;
        if (_timeUntilDirectionChange <= 0)
        {
            _direction = GetRandomDirection();
            _timeUntilDirectionChange = DirectionChangeInterval;
        }

        _position += _direction * Speed * deltaTime;

        var size = _spider.DesiredSize;

        if (_position.X < -size.X)
            _position.X = currentScreenSize.X;
        if (_position.X > currentScreenSize.X)
            _position.X = -size.X;
        if (_position.Y < -size.Y)
            _position.Y = currentScreenSize.Y;
        if (_position.Y > currentScreenSize.Y)
            _position.Y = -size.Y;

        var rotation = MathF.Atan2(_direction.X, -_direction.Y);
        _spider.Rotation = rotation;
        _spider.UpdateAnimation(deltaTime);

        LayoutContainer.SetPosition(_spider, _position);
    }

    private Vector2 GetRandomDirection()
    {
        var angle = _random.NextFloat() * MathF.Tau;
        return new Vector2(MathF.Cos(angle), MathF.Sin(angle));
    }
}

public sealed class SpiderWidget : Control
{
    private RSI.State? _aliveState;
    private int _currentFrame;
    private float _frameTime;

    private const float FrameDuration = 0.2f;
    private const int FrameCount = 7;
    private readonly Vector2 _textureScale = new(1.5f, 1.5f);

    public float Rotation { get; set; }

    public SpiderWidget()
    {
        var resourceCache = IoCManager.Resolve<IResourceCache>();
        var rsi = resourceCache.GetResource<RSIResource>("/Textures/_Goobstation/MisandryBox/spider.rsi");

        rsi.RSI.TryGetState("alive", out _aliveState);

        MouseFilter = MouseFilterMode.Ignore;

        InvalidateMeasure();
        InvalidateArrange();
    }

    public void UpdateAnimation(float deltaTime)
    {
        _frameTime += deltaTime;

        if (_frameTime >= FrameDuration)
        {
            _frameTime -= FrameDuration;
            _currentFrame = (_currentFrame + 1) % FrameCount;
        }
    }

    protected override Vector2 MeasureOverride(Vector2 availableSize)
    {
        if (_aliveState == null)
            return Vector2.Zero;

        var texture = _aliveState.GetFrame(RsiDirection.South, 0);
        return texture.Size * _textureScale;
    }

    protected override Vector2 ArrangeOverride(Vector2 finalSize)
    {
        return DesiredSize;
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        if (_aliveState == null)
            return;

        var texture = _aliveState.GetFrame(RsiDirection.South, _currentFrame);
        var textureSize = texture.Size * _textureScale;
        var center = Size / 2f;
        var topLeft = center - textureSize / 2f;
        var textureRect = UIBox2.FromDimensions(topLeft, textureSize);

        var oldTransform = handle.GetTransform();
        handle.SetTransform(Matrix3x2.CreateRotation(Rotation, center) * oldTransform);
        handle.DrawTextureRect(texture, textureRect);
        handle.SetTransform(oldTransform);
    }
}
