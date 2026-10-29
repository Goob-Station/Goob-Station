using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Input;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Goobstation.UIKit.UserInterface.Lobby;

public sealed partial class LobbyEasterEgg : LayoutContainer
{
    [Dependency] private IResourceCache _cache = default!;
    [Dependency] private IEntityManager _entMan = default!;
    [Dependency] private IInputManager _input = default!;
    [Dependency] private IClyde _clyde = default!;

    private const string DrawerPath = "/Textures/_Goobstation/Interface/Lobby/drawer.png";
    private const string WrenchPath = "/Textures/Objects/Tools/wrench.rsi";
    private const float HandleWidth = 5;
    private const float SlideSpeed = 260;

    private static readonly Vector2 WrenchSlot = new(17, 7);
    private static readonly Vector2 WrenchHotspot = new(7, 7);
    private static readonly SoundSpecifier OpenSound = new SoundPathSpecifier("/Audio/Effects/closetopen.ogg");
    private static readonly SoundSpecifier CloseSound = new SoundPathSpecifier("/Audio/Effects/closetclose.ogg");
    private static readonly SoundSpecifier PickupSound = new SoundPathSpecifier("/Audio/_Goobstation/Items/handling/wrench_pickup.ogg");
    private static readonly SoundSpecifier DropSound = new SoundPathSpecifier("/Audio/Items/wrench_drop.ogg");

    private readonly Texture _drawerTexture;
    private readonly Texture _wrench;
    private readonly DrawerControl _drawer;

    private Control? _edge;
    private MetalPanelHatch? _hatch;
    private Control? _shownWith;
    private bool _open;
    private bool _wrenchHeld;
    private float _slide = HandleWidth;

    public LobbyEasterEgg()
    {
        IoCManager.InjectDependencies(this);
        MouseFilter = MouseFilterMode.Ignore;

        _drawerTexture = _cache.GetResource<TextureResource>(DrawerPath).Texture;
        _wrench = _cache.GetResource<RSIResource>(WrenchPath).RSI.TryGetState("icon", out var state) ? state.Frame0 : Texture.Transparent;

        _drawer = new DrawerControl(this) { Visible = false };
        AddChild(_drawer);

        _input.UIKeyBindStateChanged += OnKeyBind;

        var cursor = new WrenchCursorControl(this);
        AddChild(cursor);
        SetAnchorPreset(cursor, LayoutPreset.Wide);
    }

    public void Attach(Control edge, MetalPanelHatch hatch, Control shownWith)
    {
        _edge = edge;
        _hatch = hatch;
        _shownWith = shownWith;
    }

    public void Reset()
    {
        if (_wrenchHeld)
            SetWrenchHeld(false);

        _open = false;
        _slide = HandleWidth;
        _hatch?.Reset();
    }

    private Vector2 DrawerSize => (Vector2) _drawerTexture.Size;

    private bool WrenchInDrawer => !_wrenchHeld;

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        if (_edge == null || _hatch == null)
            return;

        var shown = _shownWith?.Visible != false && _hatch.VisibleInTree;
        _drawer.Visible = shown;
        if (!shown)
        {
            if (_wrenchHeld)
                SetWrenchHeld(false);

            _open = false;
            _slide = HandleWidth;
            return;
        }

        var target = _open ? DrawerSize.X : HandleWidth;
        var step = SlideSpeed * args.DeltaSeconds;
        _slide = _slide < target ? MathF.Min(_slide + step, target) : MathF.Max(_slide - step, target);

        var edgeX = _edge.GlobalPosition.X - GlobalPosition.X;
        var y = _hatch.GlobalPosition.Y - GlobalPosition.Y + (_hatch.Size.Y - DrawerSize.Y) / 2f;
        SetMarginLeft(_drawer, edgeX - _slide);
        SetMarginTop(_drawer, y);
        SetMarginRight(_drawer, edgeX);
        SetMarginBottom(_drawer, y + DrawerSize.Y);
    }

    protected override void ExitedTree()
    {
        base.ExitedTree();

        if (_wrenchHeld)
            SetWrenchHeld(false);
    }

    private bool OnKeyBind(BoundKeyEventArgs args)
    {
        if (!_wrenchHeld || args.Function != EngineKeyFunctions.UIRightClick || args.State != BoundKeyState.Down)
            return false;

        SetWrenchHeld(false);
        Play(DropSound);
        return true;
    }

    private void Clicked(Vector2 position)
    {
        if (!_open)
        {
            _open = true;
            Play(OpenSound);
            return;
        }

        var wrenchBox = UIBox2.FromDimensions(WrenchSlot, (Vector2) _wrench.Size);
        if (WrenchInDrawer && wrenchBox.Contains(position))
        {
            SetWrenchHeld(true);
            Play(PickupSound);
            return;
        }

        if (_wrenchHeld)
        {
            SetWrenchHeld(false);
            Play(DropSound);
            return;
        }

        _open = false;
        Play(CloseSound);
    }

    private void SetWrenchHeld(bool held)
    {
        _wrenchHeld = held;
        _clyde.MainWindow.SetRelativeMouseMode(held);
        if (_hatch != null)
            _hatch.WrenchHeld = held;
    }

    private void Play(SoundSpecifier sound)
    {
        _entMan.System<SharedAudioSystem>().PlayGlobal(sound, Filter.Local(), false);
    }

    private sealed class WrenchCursorControl : Control
    {
        private readonly LobbyEasterEgg _owner;

        public WrenchCursorControl(LobbyEasterEgg owner)
        {
            _owner = owner;
            MouseFilter = MouseFilterMode.Ignore;
        }

        protected override void Draw(DrawingHandleScreen handle)
        {
            base.Draw(handle);

            if (_owner._hatch is { } hatch && _owner._drawer.Visible)
                hatch.DrawOnTop(handle, hatch.GlobalPixelPosition - GlobalPixelPosition);

            if (!_owner._wrenchHeld)
                return;

            var mouse = (UserInterfaceManager.MousePositionScaled.Position - GlobalPosition) * UIScale;
            var size = (Vector2) _owner._wrench.Size * UIScale;
            handle.DrawTextureRect(_owner._wrench, UIBox2.FromDimensions(mouse - WrenchHotspot * UIScale, size));
        }
    }

    private sealed class DrawerControl : Control
    {
        private readonly LobbyEasterEgg _owner;

        public DrawerControl(LobbyEasterEgg owner)
        {
            _owner = owner;
            MouseFilter = MouseFilterMode.Stop;
            OnKeyBindDown += args =>
            {
                if (args.Function != EngineKeyFunctions.UIClick)
                    return;

                _owner.Clicked(args.RelativePixelPosition / UIScale);
                args.Handle();
            };
        }

        protected override void Draw(DrawingHandleScreen handle)
        {
            base.Draw(handle);

            var scale = UIScale;
            var visible = Size.X;
            var texture = _owner._drawerTexture;
            var height = texture.Size.Y;

            handle.DrawTextureRectRegion(texture,
                UIBox2.FromDimensions(Vector2.Zero, new Vector2(visible, height) * scale),
                UIBox2.FromDimensions(Vector2.Zero, new Vector2(visible, height)));

            if (!_owner.WrenchInDrawer)
                return;

            var wrench = _owner._wrench;
            var shown = Math.Clamp(visible - WrenchSlot.X, 0f, wrench.Size.X);
            if (shown <= 0)
                return;

            handle.DrawTextureRectRegion(wrench,
                UIBox2.FromDimensions(WrenchSlot * scale, new Vector2(shown, wrench.Size.Y) * scale),
                UIBox2.FromDimensions(Vector2.Zero, new Vector2(shown, wrench.Size.Y)));
        }
    }
}
