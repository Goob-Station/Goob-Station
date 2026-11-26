using Content.Goobstation.Shared.Bongle;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Goobstation.Client.Bongle;

public sealed partial class SeethingSystemClient : EntitySystem
{
    [Dependency] private IOverlayManager _overlayMan = default!;
    [Dependency] private IPlayerManager _player = default!;

    private SeethingOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        _overlay = new SeethingOverlay();
    }

    [SubscribeLocalEvent]
    private void OnStartup(Entity<SeethingComponent> ent, ref ComponentStartup args)
    {
        if (_player.LocalEntity == ent.Owner)
            AddOverlay(ent.Comp);
    }

    [SubscribeLocalEvent]
    private void OnShutdown(Entity<SeethingComponent> ent, ref ComponentShutdown args)
    {
        if (_player.LocalEntity == ent.Owner)
            RemoveOverlay();
    }

    [SubscribeLocalEvent]
    private void OnAttached(Entity<SeethingComponent> ent, ref LocalPlayerAttachedEvent args)
    {
        AddOverlay(ent.Comp);
    }

    [SubscribeLocalEvent]
    private void OnDetached(Entity<SeethingComponent> ent, ref LocalPlayerDetachedEvent args)
    {
        RemoveOverlay();
    }

    private void AddOverlay(SeethingComponent seething)
    {
        _overlay.Seething = seething;

        if (!_overlayMan.HasOverlay<SeethingOverlay>())
        {
            _overlayMan.AddOverlay(_overlay);
        }
    }

    private void RemoveOverlay()
    {
        _overlayMan.RemoveOverlay(_overlay);
        _overlay.Seething = null;
    }
}
