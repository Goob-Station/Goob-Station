// SPDX-FileCopyrightText: 2025 Conchelle <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
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

    private readonly List<SpiderInstance> _temporarySpiders = new();
    private SpiderInstance? _permanentSpider;
    private bool _enabled;

    private ResPath _path = new ResPath("/Textures/_Goobstation/MisandryBox/spider.rsi");
    private string _state = "alive";

    private const float MinSpeed = 120f;
    private const float MaxSpeed = 360f;
    private const float MinActionInterval = 2f;
    private const float MaxActionInterval = 6f;

    private bool HasPermanentSpider => _permanentSpider != null;

    public void Toggle()
    {
        SetEnabled(!_enabled);
    }

    public void SetEnabled(bool enabled)
    {
        _enabled = enabled;

        if (!ShouldShowSpiders())
        {
            ClearTemporarySpiders();
        }
        else if (ShouldShowSpiders() && !HasAnySpiders())
        {
            var root = UIManager.RootControl;
            InitializeSpiders(root);
        }
    }

    public void AddTemporarySpider()
    {
        var root = UIManager.RootControl;
        var spider = CreateSpiderInstance(root, false);
        _temporarySpiders.Add(spider);

        _enabled = true;
    }

    public void AddPermanentSpider()
    {
        // You could opt in for a single spider
        ClearTemporarySpiders();

        var root = UIManager.RootControl;
        _permanentSpider = CreateSpiderInstance(root, true);
        _enabled = true;
    }

    public void ClearTemporarySpiders()
    {
        foreach (var spider in _temporarySpiders)
        {
            spider.Widget.Parent?.RemoveChild(spider.Widget);
        }
        _temporarySpiders.Clear();

        if (_permanentSpider == null)
            _enabled = false;
    }

    private bool ShouldShowSpiders()
    {
        if (!_conf.GetCVar(GoobCVars.SpiderFriend))
            return _enabled || HasPermanentSpider;

        _enabled = true;

        return _enabled || HasPermanentSpider;
    }

    private bool HasAnySpiders()
    {
        return _permanentSpider != null || _temporarySpiders.Count > 0;
    }

    private void InitializeSpiders(UIRoot root)
    {
        if (_permanentSpider != null)
        {
            _permanentSpider = CreateSpiderInstance(root, true);
        }
        else if (_temporarySpiders.Count == 0)
        {
            AddTemporarySpider();
        }
    }

    private SpiderInstance CreateSpiderInstance(UIRoot root, bool isPermanent)
    {
        var widget = new SpiderWidget(_path, _state);
        root.AddChild(widget);

        widget.Measure(new Vector2(float.PositiveInfinity));
        widget.InvalidateArrange();
        LayoutContainer.SetAnchorPreset(widget, LayoutContainer.LayoutPreset.Center);

        var position = new Vector2(
            _random.NextFloat() * root.Size.X,
            _random.NextFloat() * root.Size.Y
        );

        var instance = new SpiderInstance
        {
            Widget = widget,
            Position = position,
            Direction = GetRandomDirection(),
            TimeUntilActionChange = _random.NextFloat(MinActionInterval, MaxActionInterval),
            CurrentSpeed = _random.NextFloat(MinSpeed, MaxSpeed),
            IsMoving = _random.Next(2) == 0,
            LastScreenSize = root.Size,
            IsPermanent = isPermanent
        };

        LayoutContainer.SetPosition(widget, position);
        return instance;
    }

    public override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        if (!ShouldShowSpiders())
            return;

        var screen = UIManager.RootControl;

        if (!HasAnySpiders())
        {
            InitializeSpiders(screen);
            return;
        }

        if (_permanentSpider != null)
        {
            UpdateSpider(_permanentSpider, screen, args.DeltaSeconds);
        }

        foreach (var spider in _temporarySpiders)
        {
            UpdateSpider(spider, screen, args.DeltaSeconds);
        }
    }

    private void UpdateSpider(SpiderInstance spider, UIRoot root, float deltaTime)
    {
        var currentScreenSize = root.Size;

        if (spider.LastScreenSize != currentScreenSize)
        {
            spider.Position = new Vector2(
                Math.Clamp(spider.Position.X, 0, currentScreenSize.X),
                Math.Clamp(spider.Position.Y, 0, currentScreenSize.Y)
            );
            spider.LastScreenSize = currentScreenSize;
        }

        spider.TimeUntilActionChange -= deltaTime;
        if (spider.TimeUntilActionChange <= 0)
        {
            StartNewAction(spider);
        }

        if (spider.IsMoving)
        {
            spider.Position += spider.Direction * spider.CurrentSpeed * deltaTime;
        }

        var size = spider.Widget.DesiredSize;

        if (spider.Position.X < -size.X)
            spider.Position = spider.Position with { X = currentScreenSize.X };
        if (spider.Position.X > currentScreenSize.X)
            spider.Position = spider.Position with { X = -size.X };
        if (spider.Position.Y < -size.Y)
            spider.Position = spider.Position with { Y = currentScreenSize.Y };
        if (spider.Position.Y > currentScreenSize.Y)
            spider.Position = spider.Position with { Y = -size.Y };

        var rotation = MathF.Atan2(spider.Direction.X, -spider.Direction.Y);
        spider.Widget.Rotation = rotation;
        spider.Widget.UpdateAnimation(deltaTime);

        LayoutContainer.SetPosition(spider.Widget, spider.Position);
    }

    private void StartNewAction(SpiderInstance spider)
    {
        spider.IsMoving = !spider.IsMoving;

        if (spider.IsMoving)
        {
            spider.Direction = GetRandomDirection();
            spider.CurrentSpeed = _random.NextFloat(MinSpeed, MaxSpeed);
        }

        spider.TimeUntilActionChange = _random.NextFloat(MinActionInterval, MaxActionInterval);
    }

    private Vector2 GetRandomDirection()
    {
        var angle = _random.NextFloat() * MathF.Tau;
        return new Vector2(MathF.Cos(angle), MathF.Sin(angle));
    }
}

public sealed class SpiderInstance
{
    public SpiderWidget Widget { get; set; } = default!;
    public Vector2 Position { get; set; }
    public Vector2 Direction { get; set; }
    public float TimeUntilActionChange { get; set; }
    public float CurrentSpeed { get; set; }
    public bool IsMoving { get; set; }
    public Vector2 LastScreenSize { get; set; }
    public bool IsPermanent { get; set; }
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
