using Content.Goobstation.Shared.Cyberdeck;
using Content.Goobstation.Shared.Cyberdeck.Components;
using Robust.Client.Graphics;
using Robust.Shared.Player;

namespace Content.Goobstation.Client.Cyberdeck;

public sealed class CyberdeckSystem : SharedCyberdeckSystem
{
    [Dependency] private readonly IOverlayManager _overlayManager = default!;

    private CyberdeckOverlay _overlay = default!;
    private EntityQuery<CyberdeckProjectionComponent> _users;

    public override void Initialize()
    {
        base.Initialize();

        _overlay = new CyberdeckOverlay();
        _users = GetEntityQuery<CyberdeckProjectionComponent>();

        SubscribeLocalEvent<LocalPlayerAttachedEvent>(OnPlayerAttach);
        SubscribeLocalEvent<LocalPlayerDetachedEvent>(OnPlayerDetached);
    }

    private void OnPlayerAttach(LocalPlayerAttachedEvent args)
    {
        if (!_users.HasComp(args.Entity))
            return;

        _overlayManager.AddOverlay(_overlay);
    }

    private void OnPlayerDetached(LocalPlayerDetachedEvent args)
    {
        _overlayManager.RemoveOverlay(_overlay);
    }

    protected override void ShutdownProjection(Entity<CyberdeckProjectionComponent?>? ent) { }
}
