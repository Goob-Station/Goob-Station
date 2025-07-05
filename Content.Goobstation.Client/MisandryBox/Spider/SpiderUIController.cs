// SPDX-FileCopyrightText: 2025 Conchelle <mary@thughunt.ing>
//
// SPDX-License-Identifier: MPL-2.0

using System.Numerics;
using Content.Client.Lobby;
using Content.Goobstation.Common.CCVar;
using Robust.Client;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Configuration;
using Robust.Shared.Console;
using Robust.Shared.Graphics.RSI;
using Robust.Shared.Network;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.MisandryBox.Spider;

public sealed class SpiderUIController : UIController
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IConfigurationManager _conf = default!;

    private SpiderWidget? _spider;
    private Vector2 _position;
    private Vector2 _direction;
    private float _timeUntilActionChange;
    private float _currentSpeed;
    private bool _enabled;
    private bool _isMoving;
    private Vector2 _lastScreenSize;

    private ResPath _path = new ResPath("/Textures/_Goobstation/MisandryBox/spider.rsi");
    private string _state = "alive";

    public bool Permanent;

    private const float MinSpeed = 120f;
    private const float MaxSpeed = 540f;
    private const float MinActionInterval = 2f;
    private const float MaxActionInterval = 10f;

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
            var root = UIManager.RootControl;
            InitializeSpider(root);
        }
    }

    private bool ShouldShowSpider()
    {
        if (!_conf.GetCVar(GoobCVars.SpiderFriend))
            return _enabled || Permanent;

        _enabled = true;
        Permanent = true;

        return _enabled || Permanent;
    }

    public override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        if (!ShouldShowSpider())
            return;

        var screen = UIManager.RootControl;

        if (_spider == null)
        {
            InitializeSpider(screen);
            return;
        }

        UpdateSpider(screen, args.DeltaSeconds);
    }

    private void InitializeSpider(UIRoot root, ResPath? path = null, string? state = null)
    {
        path ??= _path;
        state ??= _state;
        _spider = new SpiderWidget(path.Value, state);

        root.AddChild(_spider);

        _spider.Measure(new Vector2(float.PositiveInfinity));
        _spider.InvalidateArrange();
        LayoutContainer.SetAnchorPreset(_spider, LayoutContainer.LayoutPreset.Center);
        LayoutContainer.SetPosition(_spider, _position);

        _lastScreenSize = root.Size;
        _position = new Vector2(
            _random.NextFloat() * root.Size.X,
            _random.NextFloat() * root.Size.Y
        );

        StartNewAction();
    }

    private void UpdateSpider(UIRoot root, float deltaTime)
    {
        if (_spider == null)
            return;

        var currentScreenSize = root.Size;

        if (_lastScreenSize != currentScreenSize)
        {
            _position.X = Math.Clamp(_position.X, 0, currentScreenSize.X);
            _position.Y = Math.Clamp(_position.Y, 0, currentScreenSize.Y);
            _lastScreenSize = currentScreenSize;
        }

        _timeUntilActionChange -= deltaTime;
        if (_timeUntilActionChange <= 0)
        {
            StartNewAction();
        }

        if (_isMoving)
        {
            _position += _direction * _currentSpeed * deltaTime;
        }

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

    private void StartNewAction()
    {
        _isMoving = !_isMoving;

        if (_isMoving)
        {
            _direction = GetRandomDirection();
            _currentSpeed = _random.NextFloat(MinSpeed, MaxSpeed);
        }

        _timeUntilActionChange = _random.NextFloat(MinActionInterval, MaxActionInterval);
    }

    private Vector2 GetRandomDirection()
    {
        var angle = _random.NextFloat() * MathF.Tau;
        return new Vector2(MathF.Cos(angle), MathF.Sin(angle));
    }
}

public sealed class SpiderWidget : Control
{
    private RSI.State? _state;
    private int _currentFrame;
    private float _frameTime;

    private float _frameDuration;
    private int _frameCount;
    private readonly Vector2 _textureScale = new(1.5f, 1.5f);

    public float Rotation { get; set; }

    public SpiderWidget(ResPath path, string state)
    {
        var resourceCache = IoCManager.Resolve<IResourceCache>();
        var rsi = resourceCache.GetResource<RSIResource>(path);

        if (!rsi.RSI.TryGetState(state, out _state))
            return;

        _frameDuration = _state.GetDelay(0);
        _frameCount = _state.DelayCount;

        MouseFilter = MouseFilterMode.Ignore;

        InvalidateMeasure();
        InvalidateArrange();
    }

    public void UpdateAnimation(float deltaTime)
    {
        _frameTime += deltaTime;

        if (_frameTime >= _frameDuration)
        {
            _frameTime -= _frameDuration;
            _currentFrame = (_currentFrame + 1) % _frameCount;
        }
    }

    protected override Vector2 MeasureOverride(Vector2 availableSize)
    {
        if (_state == null)
            return Vector2.Zero;

        var texture = _state.GetFrame(RsiDirection.South, 0);
        return texture.Size * _textureScale;
    }

    protected override Vector2 ArrangeOverride(Vector2 finalSize)
    {
        return DesiredSize;
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        if (_state == null)
            return;

        var texture = _state.GetFrame(RsiDirection.South, _currentFrame);
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
