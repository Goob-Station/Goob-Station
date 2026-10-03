using Content.Goobstation.Shared.Vision;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Containers;
using Robust.Shared.Player;

namespace Content.Goobstation.Client.Vision;

public sealed class RestrictFovOverlaySystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly IPlayerManager _playerMan = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;

    private readonly RestrictFovOverlay _overlay = new();
    private EntityQuery<RestrictFovComponent> _fovQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RestrictFovComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<RestrictFovComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<RestrictFovComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<RestrictFovComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

        SubscribeLocalEvent<RestrictFovComponent, EntInsertedIntoContainerMessage>(OnInserted);
        SubscribeLocalEvent<RestrictFovComponent, EntRemovedFromContainerMessage>(OnRemoved);

        _fovQuery = GetEntityQuery<RestrictFovComponent>();
    }

    private void OnInit(Entity<RestrictFovComponent> ent, ref ComponentInit args) =>
        Refresh();

    private void OnShutdown(Entity<RestrictFovComponent> ent, ref ComponentShutdown args)
    {
        if (RestrictFovOverlay.TryGetSource(_fovQuery, _container, _playerMan, out var source, out _) && source == ent.Owner)
            _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnPlayerAttached(Entity<RestrictFovComponent> ent, ref LocalPlayerAttachedEvent args) =>
        _overlayMan.AddOverlay(_overlay);

    private void OnPlayerDetached(Entity<RestrictFovComponent> ent, ref LocalPlayerDetachedEvent args) =>
        _overlayMan.RemoveOverlay(_overlay);

    private void OnInserted(Entity<RestrictFovComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (args.Entity == _playerMan.LocalEntity)
            Refresh();
    }

    private void OnRemoved(Entity<RestrictFovComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (args.Entity == _playerMan.LocalEntity)
            Refresh();
    }

    private void Refresh()
    {
        if (RestrictFovOverlay.TryGetSource(_fovQuery, _container, _playerMan, out _, out _))
        {
            if (!_overlayMan.HasOverlay<RestrictFovOverlay>())
                _overlayMan.AddOverlay(_overlay);
        }
        else
        {
            _overlayMan.RemoveOverlay(_overlay);
        }
    }
}
