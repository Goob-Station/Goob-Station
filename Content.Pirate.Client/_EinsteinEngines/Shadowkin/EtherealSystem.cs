using Content.Shared.Shadowkin;
using Robust.Client.Graphics;
using Robust.Shared.Player;
using Content.Client.Overlays;

namespace Content.Client.Shadowkin;

public sealed partial class EtherealSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly ISharedPlayerManager _playerMan = default!;

    private EtherealOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EtherealComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<EtherealComponent, ComponentShutdown>(Onhutdown);
        SubscribeLocalEvent<EtherealComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<EtherealComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

        _overlay = new();
    }

    private void OnInit(EntityUid uid, EtherealComponent component, ComponentInit args)
    {
        if (uid != _playerMan.LocalEntity)
            return;

        _overlayMan.AddOverlay(_overlay);
    }

    private void Onhutdown(EntityUid uid, EtherealComponent component, ComponentShutdown args)
    {
        if (uid != _playerMan.LocalEntity)
            return;

        _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnPlayerAttached(EntityUid uid, EtherealComponent component, LocalPlayerAttachedEvent args)
    {
        _overlayMan.AddOverlay(_overlay);
    }

    private void OnPlayerDetached(EntityUid uid, EtherealComponent component, LocalPlayerDetachedEvent args)
    {
        _overlayMan.RemoveOverlay(_overlay);
    }
}
