using System.Numerics;
using Content.Client.Eye;
using Content.Client.Viewport;
using Content.Goobstation.Shared.Twitch;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;

namespace Content.Goobstation.Client.Twitch;

public sealed class TwitchChatCritterSystem : EntitySystem
{
    private EyeLerpingSystem _eyes = default!;
    private readonly FixedEye _defaultEye = new();
    private NetEntity? _pendingCamera;
    private EntityUid? _camera;
    private DefaultWindow? _window;
    private ScalingViewport? _viewport;
    private Label? _commandLabel;

    public override void Initialize()
    {
        base.Initialize();
        _eyes = EntityManager.System<EyeLerpingSystem>();
        SubscribeNetworkEvent<TwitchChatCritterOpenEvent>(OnOpen);
        SubscribeNetworkEvent<TwitchChatCritterCommandEvent>(OnCommand);
        SubscribeNetworkEvent<TwitchChatCritterClosedEvent>(_ => CloseWindow(false));
    }

    public override void Shutdown()
    {
        CloseWindow(false);
        base.Shutdown();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_pendingCamera is not { } netCamera ||
            !EntityManager.TryGetEntity(netCamera, out var camera) ||
            camera == null ||
            !TryComp<EyeComponent>(camera, out var eye))
        {
            return;
        }

        _pendingCamera = null;
        _camera = camera;
        _eyes.AddEye(camera.Value);
        OpenWindow(eye);
    }

    private void OnOpen(TwitchChatCritterOpenEvent message)
    {
        CloseWindow(false);
        _pendingCamera = message.Camera;
    }

    private void OnCommand(TwitchChatCritterCommandEvent message)
    {
        if (_commandLabel != null)
            _commandLabel.Text = $"{message.Viewer}: {message.Command}";
    }

    private void OpenWindow(EyeComponent eye)
    {
        _viewport = new ScalingViewport
        {
            MinSize = new Vector2(340, 220),
            ViewportSize = new Vector2i(340, 220),
            VerticalExpand = true,
            HorizontalExpand = true,
            AlwaysRender = true,
            RenderScaleMode = ScalingViewportRenderScaleMode.CeilInt,
            MouseFilter = Control.MouseFilterMode.Ignore,
            Eye = eye.Eye ?? _defaultEye,
        };
        _viewport.OnResized += ResizeViewport;
        _commandLabel = new Label
        {
            Text = "Chat commands: up / down / left / right / bite",
            Margin = new Thickness(4, 7),
            ClipText = true,
        };
        var content = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            Children = { _viewport, _commandLabel },
        };
        _window = new DefaultWindow
        {
            Title = "Chat",
            MinSize = new Vector2(380, 290),
            SetSize = new Vector2(420, 330),
        };
        _window.Contents.AddChild(content);
        _window.OnClose += () => CloseWindow(true);
        _window.OpenCentered();
    }

    private void CloseWindow(bool notifyServer)
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

        if (_window?.IsOpen == true)
            _window.Close();
        _window = null;
        _viewport = null;
        _commandLabel = null;
    }

    private void ResizeViewport()
    {
        if (_viewport == null)
            return;

        var width = Math.Max(_viewport.PixelWidth, (int) MathF.Floor(_viewport.MinWidth));
        var height = Math.Max(_viewport.PixelHeight, (int) MathF.Floor(_viewport.MinHeight));
        _viewport.ViewportSize = new Vector2i(width, height);
    }
}
