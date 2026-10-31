using Content.Shared.Input;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Input;
using Robust.Shared.Input.Binding;

namespace Content.Client._ES.NamePeek;

/// <summary>
/// Shows names (and pronouns) under nearby mobs while the name peek key is held.
/// </summary>
public sealed class ESNamePeekSystem : EntitySystem
{
    [Dependency] private IOverlayManager _overlay = default!;
    [Dependency] private IPlayerManager _player = default!;

    public bool Visible;

    public override void Initialize()
    {
        base.Initialize();

        UpdatesOutsidePrediction = true;

        _overlay.AddOverlay(new ESNamePeekOverlay(this));

        CommandBinds.Builder
            .Bind(ContentKeyFunctions.NamePeek, new PointerInputCmdHandler(OnNamePeek, ignoreUp: false, outsidePrediction: true))
            .Register<ESNamePeekSystem>();
    }

    public override void Shutdown()
    {
        base.Shutdown();

        _overlay.RemoveOverlay<ESNamePeekOverlay>();
        CommandBinds.Unregister<ESNamePeekSystem>();
    }

    private bool OnNamePeek(in PointerInputCmdHandler.PointerInputCmdArgs args)
    {
        if (_player.LocalEntity == null)
            return false;

        Visible = args.State == BoundKeyState.Down;
        return false;
    }
}
