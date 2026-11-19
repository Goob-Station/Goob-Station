using Content.Goobstation.Shared.Bongle;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Goobstation.Client.Bongle;

public sealed partial class SeethingSystem : EntitySystem
{
    [Dependency] private IOverlayManager _overlayMan = default!;
    [Dependency] private IPlayerManager _player = default!;

    private SeethingOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        _overlay = new SeethingOverlay();

        SubscribeLocalEvent<SeethingComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<SeethingComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<SeethingComponent, LocalPlayerAttachedEvent>(OnAttached);
        SubscribeLocalEvent<SeethingComponent, LocalPlayerDetachedEvent>(OnDetached);
    }

    private void OnStartup(Entity<SeethingComponent> ent, ref ComponentStartup args)
    {
        var (uid, _) = ent;

        if (_player.LocalEntity == uid)
        {
            AddOverlay();
        }
    }

    private void OnShutdown(Entity<SeethingComponent> ent, ref ComponentShutdown args)
    {
        var (uid, _) = ent;

        if (_player.LocalEntity == uid)
        {
            RemoveOverlay();
        }
    }

    private void OnAttached(Entity<SeethingComponent> ent, ref LocalPlayerAttachedEvent args)
    {
        AddOverlay();
    }

    private void OnDetached(Entity<SeethingComponent> ent, ref LocalPlayerDetachedEvent args)
    {
        RemoveOverlay();
    }

    private void AddOverlay()
    {
        if (!_overlayMan.HasOverlay<SeethingOverlay>())
        {
            _overlayMan.AddOverlay(_overlay);
        }
    }

    private void RemoveOverlay()
    {
        _overlayMan.RemoveOverlay(_overlay);
    }
}
