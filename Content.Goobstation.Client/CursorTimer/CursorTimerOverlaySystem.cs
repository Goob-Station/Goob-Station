using Content.Goobstation.Shared.CursorTimer;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Goobstation.Client.CursorTimer;

public sealed partial class CursorTimerOverlaySystem : EntitySystem
{
    [Dependency] private IOverlayManager _overlayMan = default!;
    [Dependency] private IPlayerManager _player = default!;

    private readonly CursorTimerOverlay _overlay = new();

    [SubscribeLocalEvent]
    private void OnStartup(Entity<CursorTimerComponent> ent, ref ComponentStartup args)
    {
        if (_player.LocalEntity == ent.Owner)
            _overlayMan.AddOverlay(_overlay);
    }

    [SubscribeLocalEvent]
    private void OnShutdown(Entity<CursorTimerComponent> ent, ref ComponentShutdown args)
    {
        if (_player.LocalEntity == ent.Owner)
            _overlayMan.RemoveOverlay(_overlay);
    }

    [SubscribeLocalEvent]
    private void OnPlayerAttached(EntityUid uid, CursorTimerComponent comp, LocalPlayerAttachedEvent args)
    {
        _overlayMan.AddOverlay(_overlay);
    }

    [SubscribeLocalEvent]
    private void OnPlayerDetached(EntityUid uid, CursorTimerComponent comp, LocalPlayerDetachedEvent args)
    {
        _overlayMan.RemoveOverlay(_overlay);
    }
}
