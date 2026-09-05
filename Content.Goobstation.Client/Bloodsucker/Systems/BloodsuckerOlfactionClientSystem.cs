using Content.Goobstation.Client.Bloodsucker.Overlays;
using Content.Goobstation.Shared.Bloodsuckers.Components;
using Robust.Client.Graphics;
using Robust.Shared.Player;

namespace Content.Goobstation.Client.Bloodsucker.Systems;

public sealed class BloodsuckerOlfactionClientSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;

    private BloodsuckerOlfactionOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();
        _overlay = new BloodsuckerOlfactionOverlay();
        IoCManager.InjectDependencies(_overlay);

        SubscribeLocalEvent<BloodsuckerOlfactionOverlayComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<BloodsuckerOlfactionOverlayComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<BloodsuckerOlfactionOverlayComponent, LocalPlayerAttachedEvent>(OnAttached);
        SubscribeLocalEvent<BloodsuckerOlfactionOverlayComponent, LocalPlayerDetachedEvent>(OnDetached);
    }

    private void OnInit(Entity<BloodsuckerOlfactionOverlayComponent> ent, ref ComponentInit args)
    {
        if (ent.Owner == _player.LocalEntity)
            _overlayMan.AddOverlay(_overlay);
    }

    private void OnShutdown(Entity<BloodsuckerOlfactionOverlayComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Owner == _player.LocalEntity)
            _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnAttached(Entity<BloodsuckerOlfactionOverlayComponent> ent, ref LocalPlayerAttachedEvent args)
        => _overlayMan.AddOverlay(_overlay);

    private void OnDetached(Entity<BloodsuckerOlfactionOverlayComponent> ent, ref LocalPlayerDetachedEvent args)
        => _overlayMan.RemoveOverlay(_overlay);
}
