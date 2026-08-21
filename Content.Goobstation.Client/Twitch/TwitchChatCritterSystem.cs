using System.Numerics;
using Content.Client.Eye;
using Content.Client.UserInterface.Systems.Chat.Widgets;
using Content.Client.Viewport;
using Content.Goobstation.Shared.Twitch;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Twitch;

public sealed class TwitchChatCritterSystem : EntitySystem
{
    private static readonly Vector2 ChatZoom = new(0.4f, 0.4f);

    [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private SharedEyeSystem _eye = default!;
    private EyeLerpingSystem _eyes = default!;
    private readonly FixedEye _defaultEye = new();
    private NetEntity? _pendingCamera;
    private EntityUid? _camera;
    private BoxContainer? _host;
    private BoxContainer? _content;
    private ScalingViewport? _viewport;
    private Label? _commandLabel;
    private Label? _timerLabel;
    private TimeSpan _expiresAt;

    public override void Initialize()
    {
        base.Initialize();
        _eye = EntityManager.System<SharedEyeSystem>();
        _eyes = EntityManager.System<EyeLerpingSystem>();
        SubscribeNetworkEvent<TwitchChatCritterOpenEvent>(OnOpen);
        SubscribeNetworkEvent<TwitchChatCritterCommandEvent>(OnCommand);
        SubscribeNetworkEvent<TwitchChatCritterClosedEvent>(_ => ClosePanel());
    }

    public override void Shutdown()
    {
        ClosePanel();
        base.Shutdown();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        UpdateTimer();

        if (_pendingCamera is { } netCamera)
        {
            if (!EntityManager.TryGetEntity(netCamera, out var camera) ||
                camera == null ||
                !TryComp<EyeComponent>(camera, out var pendingEye))
            {
                return;
            }

            _pendingCamera = null;
            _camera = camera;
            _eye.SetZoom(camera.Value, ChatZoom, pendingEye);
            _eye.SetDrawFov(camera.Value, true, pendingEye);
            _eyes.AddEye(camera.Value, pendingEye);
        }

        if (_camera is not { } activeCamera ||
            !Exists(activeCamera) ||
            !TryComp<EyeComponent>(activeCamera, out var eye))
        {
            return;
        }

        var host = GetHost();
        if (host == null || host == _host && _content?.Parent == host)
            return;

        MountPanel(host, eye);
    }

    private void OnOpen(TwitchChatCritterOpenEvent message)
    {
        ClosePanel();
        _expiresAt = message.ExpiresAt;
        _pendingCamera = message.Camera;
    }

    private void OnCommand(TwitchChatCritterCommandEvent message)
    {
        if (_commandLabel != null)
            _commandLabel.Text = $"{message.Viewer}: {message.Command}";
    }

    private BoxContainer? GetHost()
    {
        var chat = _uiManager.ActiveScreen?.GetWidget<ChatBox>() ??
                   _uiManager.ActiveScreen?.GetWidget<ResizableChatBox>();
        return chat?.TwitchChatCritterContainer;
    }

    private void MountPanel(BoxContainer host, EyeComponent eye)
    {
        UnmountPanel();
        _host = host;
        _viewport = new ScalingViewport
        {
            MinSize = new Vector2(0, 180),
            ViewportSize = new Vector2i(320, 180),
            HorizontalExpand = true,
            AlwaysRender = true,
            RenderScaleMode = ScalingViewportRenderScaleMode.Fixed,
            FixedRenderScale = 1,
            StretchMode = ScalingViewportStretchMode.Nearest,
            MouseFilter = Control.MouseFilterMode.Ignore,
            Eye = eye.Eye ?? _defaultEye,
        };
        _viewport.OnResized += ResizeViewport;
        _commandLabel = new Label
        {
            Text = "Commands: up / down / left / right / bite",
            HorizontalExpand = true,
            ClipText = true,
        };
        _timerLabel = new Label
        {
            MinSize = new Vector2(44, 24),
            HorizontalAlignment = Control.HAlignment.Center,
            VerticalAlignment = Control.VAlignment.Center,
        };
        UpdateTimer();
        var header = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            Children =
            {
                _commandLabel,
                _timerLabel,
            },
        };
        _content = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
            Children =
            {
                header,
                _viewport,
            },
        };
        host.AddChild(_content);
        host.Visible = true;
    }

    private void UpdateTimer()
    {
        if (_timerLabel == null)
            return;

        var seconds = Math.Max(0, (int) Math.Ceiling((_expiresAt - _timing.CurTime).TotalSeconds));
        _timerLabel.Text = $"{seconds / 60}:{seconds % 60:00}";
    }

    private void ClosePanel()
    {
        var camera = _camera;
        _pendingCamera = null;
        _camera = null;
        _expiresAt = TimeSpan.Zero;

        if (camera is { } entity && Exists(entity))
            _eyes.RemoveEye(entity);

        UnmountPanel();
    }

    private void UnmountPanel()
    {
        _content?.Orphan();
        if (_host != null)
            _host.Visible = false;
        _host = null;
        _content = null;
        _viewport = null;
        _commandLabel = null;
        _timerLabel = null;
    }

    private void ResizeViewport()
    {
        if (_viewport == null)
            return;

        if (_viewport.PixelWidth <= 0 || _viewport.PixelHeight <= 0)
            return;

        var width = _viewport.PixelWidth;
        var height = _viewport.PixelHeight;
        _viewport.ViewportSize = new Vector2i(width, height);
    }
}
