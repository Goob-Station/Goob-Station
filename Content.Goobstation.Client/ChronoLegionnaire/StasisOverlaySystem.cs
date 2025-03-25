using Content.Goobstation.Client.ChronoLegionnaire.Overlays;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Goobstation.Client.ChronoLegionnaire;

public sealed class StasisOverlaySystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IOverlayManager _overlayManager = default!;

    private StasisOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<Shared.ChronoLegionnaire.Components.InsideStasisComponent, ComponentInit>(OnStasisInit);
        SubscribeLocalEvent<Shared.ChronoLegionnaire.Components.InsideStasisComponent, ComponentShutdown>(OnStasisShutdown);

        SubscribeLocalEvent<Shared.ChronoLegionnaire.Components.InsideStasisComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<Shared.ChronoLegionnaire.Components.InsideStasisComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

        _overlay = new();
    }

    private void OnPlayerAttached(Entity<Shared.ChronoLegionnaire.Components.InsideStasisComponent> stasised, ref LocalPlayerAttachedEvent args)
    {
        _overlayManager.AddOverlay(_overlay);
    }

    private void OnPlayerDetached(Entity<Shared.ChronoLegionnaire.Components.InsideStasisComponent> stasised, ref LocalPlayerDetachedEvent args)
    {
        _overlayManager.RemoveOverlay(_overlay);
    }

    private void OnStasisInit(Entity<Shared.ChronoLegionnaire.Components.InsideStasisComponent> stasised, ref ComponentInit args)
    {
        if (_player.LocalEntity == stasised)
            _overlayManager.AddOverlay(_overlay);
    }

    private void OnStasisShutdown(Entity<Shared.ChronoLegionnaire.Components.InsideStasisComponent> stasised, ref ComponentShutdown args)
    {
        if (_player.LocalEntity == stasised)
        {
            _overlayManager.RemoveOverlay(_overlay);
        }
    }
}
