using Content.Goobstation.Shared.Mech.Components;
using Robust.Client.Graphics;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Mech;

public sealed class MechStompOverlaySystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private readonly MechStompOverlay _overlay = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MechStompOverlayComponent, ComponentInit>(OnStompInit);
        SubscribeLocalEvent<MechStompOverlayComponent, ComponentShutdown>(OnStompShutdown);
    }

    private void OnStompInit(Entity<MechStompOverlayComponent> ent, ref ComponentInit args)
    {
        ent.Comp.LocalStartTime = _timing.RealTime;

        _overlayMan.AddOverlay(_overlay);
    }

    private void OnStompShutdown(Entity<MechStompOverlayComponent> ent, ref ComponentShutdown args)
    {
        if (Count<MechStompOverlayComponent>() <= 1)
            _overlayMan.RemoveOverlay(_overlay);
    }
}
