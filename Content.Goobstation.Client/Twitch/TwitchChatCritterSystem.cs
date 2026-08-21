using System.Numerics;
using Content.Client.Eye;
using Content.Client.UserInterface.Systems.Chat.Widgets;
using Content.Client.Viewport;
using Content.Goobstation.Shared.Twitch;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Goobstation.Client.Twitch;

public sealed class TwitchChatCritterSystem : EntitySystem
{
    private static readonly Vector2 ChatZoom = new(0.4f, 0.4f);

    [Dependency] private readonly IUserInterfaceManager _uiManager = default!;

    private SharedEyeSystem _eye = default!;
    private EyeLerpingSystem _eyes = default!;
    private readonly FixedEye _defaultEye = new();
    private NetEntity? _pendingCamera;
    private EntityUid? _camera;
    private BoxContainer? _host;
    private BoxContainer? _content;
    private ScalingViewport? _viewport;
    private Label? _commandLabel;

    public override void Initialize()
    {
        base.Initialize();
        _eye = EntityManager.System<SharedEyeSystem>();
        _eyes = EntityManager.System<EyeLerpingSystem>();
        SubscribeNetworkEvent<TwitchChatCritterOpenEvent>(OnOpen);
        SubscribeNetworkEvent<TwitchChatCritterCommandEvent>(OnCommand);
        SubscribeNetworkEvent<TwitchChatCritterClosedEvent>(_ => ClosePanel(false));
    }

    public override void Shutdown()
    {
        ClosePanel(false);
        base.Shutdown();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

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
        ClosePanel(false);
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
        var close = new Button
        {
            Text = "×",
            MinSize = new Vector2(28, 24),
        };
        close.OnPressed += _ => ClosePanel(true);
        var header = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            Children =
            {
                _commandLabel,
                close,
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

    private void ClosePanel(bool notifyServer)
    {
        var camera = _camera;
        _pendingCamera = null;
        _camera = null;

        if (camera is { } entity && Exists(entity))
        {
            _eyes.RemoveEye(entity);
            if (notifyServer)
                RaiseNetworkEvent(new TwitchChatCritterCloseEvent(GetNetEntity(entity)));
        }

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
