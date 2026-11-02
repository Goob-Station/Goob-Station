using Content.Goobstation.Shared.Vision;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Goobstation.Client.Vision;

public sealed partial class RestrictFovOverlaySystem : EntitySystem
{
    [Dependency] private IOverlayManager _overlayMan = default!;
    [Dependency] private IPlayerManager _playerMan = default!;

    private readonly RestrictFovOverlay _overlay = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RestrictFovComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<RestrictFovComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<RestrictFovComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<RestrictFovComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);
    }

    private void OnInit(Entity<RestrictFovComponent> ent, ref ComponentInit args)
    {
        if (ent.Owner == _playerMan.LocalEntity)
            _overlayMan.AddOverlay(_overlay);
    }

    private void OnShutdown(Entity<RestrictFovComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Owner == _playerMan.LocalEntity)
            _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnPlayerAttached(Entity<RestrictFovComponent> ent, ref LocalPlayerAttachedEvent args) =>
        _overlayMan.AddOverlay(_overlay);

    private void OnPlayerDetached(Entity<RestrictFovComponent> ent, ref LocalPlayerDetachedEvent args) =>
        _overlayMan.RemoveOverlay(_overlay);
}
