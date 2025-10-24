using Content.Goobstation.Shared.Wraith.Other;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Goobstation.Client.Wraith.Insanity;

public sealed class InsanitySystem : EntitySystem
{
    private InsanityOverlay _overlay = default!;

    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IOverlayManager _overlayManager = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WraithInsanityComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<WraithInsanityComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<WraithInsanityComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<WraithInsanityComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

        _overlay = new();
    }

    private void OnPlayerAttached(Entity<WraithInsanityComponent> ent, ref LocalPlayerAttachedEvent args) =>
        _overlayManager.AddOverlay(_overlay);

    private void OnPlayerDetached(Entity<WraithInsanityComponent> ent, ref LocalPlayerDetachedEvent args) =>
        _overlayManager.RemoveOverlay(_overlay);

    private void OnInit(Entity<WraithInsanityComponent> ent, ref ComponentInit args)
    {
        if (_playerManager.LocalEntity != ent.Owner)
            return;

        _overlay.SetValues(ent.Comp.Speed, ent.Comp.Radius, ent.Comp.Color);
        _overlayManager.AddOverlay(_overlay);
    }

    private void OnShutdown(Entity<WraithInsanityComponent> ent, ref ComponentShutdown args)
    {
        if (_playerManager.LocalEntity != ent.Owner)
            return;

        _overlayManager.RemoveOverlay(_overlay);
    }
}
