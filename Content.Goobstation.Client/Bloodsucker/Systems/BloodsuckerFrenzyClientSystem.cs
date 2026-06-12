using Content.Goobstation.Client.Bloodsucker.Overlays;
using Content.Goobstation.Shared.Bloodsuckers.Components;
using Robust.Client.Graphics;
using Robust.Shared.Player;

namespace Content.Goobstation.Client.Bloodsucker.Systems;

public sealed class BloodsuckerFrenzyClientSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;

    private BloodsuckerFrenzyOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();
        _overlay = new BloodsuckerFrenzyOverlay();
        //IoCManager.InjectDependencies(_overlay);

        SubscribeLocalEvent<BloodsuckerFrenzyOverlayComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<BloodsuckerFrenzyOverlayComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<BloodsuckerFrenzyOverlayComponent, LocalPlayerAttachedEvent>(OnAttached);
        SubscribeLocalEvent<BloodsuckerFrenzyOverlayComponent, LocalPlayerDetachedEvent>(OnDetached);
    }

    private void OnInit(Entity<BloodsuckerFrenzyOverlayComponent> ent, ref ComponentInit args)
    {
        if (ent.Owner == _player.LocalEntity)
            _overlayMan.AddOverlay(_overlay);
    }

    private void OnShutdown(Entity<BloodsuckerFrenzyOverlayComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Owner == _player.LocalEntity)
            _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnAttached(Entity<BloodsuckerFrenzyOverlayComponent> ent, ref LocalPlayerAttachedEvent args)
        => _overlayMan.AddOverlay(_overlay);

    private void OnDetached(Entity<BloodsuckerFrenzyOverlayComponent> ent, ref LocalPlayerDetachedEvent args)
        => _overlayMan.RemoveOverlay(_overlay);
}
