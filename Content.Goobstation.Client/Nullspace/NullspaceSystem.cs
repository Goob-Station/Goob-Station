using Content.Client.Eye;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Shared.Nullspace;
using Robust.Client;
using Robust.Client.Audio;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Nullspace;

public sealed partial class NullspaceSystem : SharedNullspaceSystem
{
    [Dependency] private IClyde _clyde = default!;
    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private IGameController _game = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private IPrototypeManager _prototypes = default!;
    [Dependency] private IResourceCache _resources = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private ITileDefinitionManager _tiles = default!;
    [Dependency] private IUserInterfaceManager _ui = default!;
    [Dependency] private SharedEyeSystem _eye = default!;
    [Dependency] private SharedTransformSystem _transform = default!;

    private readonly NullspaceFrame _frame = new();
    private NullspaceRenderer? _renderer;
    private NullspaceScreen? _screen;
    private Control? _hiddenScreen;
    private EntityUid? _viewer;

    public override void Initialize()
    {
        UpdatesAfter.Add(typeof(TransformSystem));
        UpdatesAfter.Add(typeof(EyeLerpingSystem));
        UpdatesBefore.Add(typeof(AudioSystem));
        base.Initialize();

        Subs.CVar(_cfg, GoobCVars.NullspaceSensitivity, value => _sensitivity = value, true);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        Close();
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        if (_player.LocalEntity is not { } uid
            || !TryComp<NullspacePlayerComponent>(uid, out var player)
            || !TryGetMap(uid, out var map))
        {
            Close();
            return;
        }

        if (_viewer != uid)
        {
            Close();
            Open(uid);
        }

        KeepScreen();

        var maze = GetMaze(map.Comp);
        var dt = MathF.Max(frameTime, 1e-4f);

        UpdateCapture();
        UpdateLook((uid, player), player.Frozen || _dying);
        UpdateView((uid, player), map, maze, dt);
    }

    private void Open(EntityUid uid)
    {
        _viewer = uid;
        _renderer = new NullspaceRenderer(_clyde, EntityManager, _prototypes, _resources, _tiles);
        _screen = new NullspaceScreen(this, _renderer, _frame);
        ResetView(uid);
        KeepScreen();
    }

    private void KeepScreen()
    {
        if (_screen == null)
            return;

        var active = _ui.ActiveScreen;
        if (active != _hiddenScreen)
        {
            if (_hiddenScreen != null)
                _hiddenScreen.Visible = true;

            _hiddenScreen = active;
        }

        if (_hiddenScreen != null)
            _hiddenScreen.Visible = false;

        var parent = _hiddenScreen?.Parent ?? _ui.StateRoot;
        if (_screen.Parent != parent)
        {
            _screen.Orphan();
            parent.AddChild(_screen);
            if (parent is LayoutContainer)
                LayoutContainer.SetAnchorPreset(_screen, LayoutContainer.LayoutPreset.Wide);
        }

        if (_screen.GetPositionInParent() != parent.ChildCount - 1)
            _screen.SetPositionLast();

        if (_ui.KeyboardFocused is { } focused && !focused.VisibleInTree)
            _ui.ReleaseKeyboardFocus();
    }

    private void Close()
    {
        if (_viewer == null)
            return;

        _viewer = null;
        _captured = false;
        SetRelative(false);
        if (_screen != null && _ui.ControlFocused == _screen)
            _ui.ControlFocused = null;
        StopAudio();

        _screen?.Orphan();
        _screen = null;

        if (_hiddenScreen != null)
            _hiddenScreen.Visible = true;
        _hiddenScreen = null;

        _renderer?.Dispose();
        _renderer = null;
    }
}
