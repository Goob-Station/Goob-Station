using Content.Goobstation.Client.Slasher.Overlays;
using Content.Goobstation.Shared.Slasher.Components;
using Robust.Client.Graphics;

namespace Content.Goobstation.Client.Slasher.Systems;

/// <summary>
/// Registers the HeartEyesOverlay while any entity has heart eyes.
/// </summary>
public sealed class HeartEyesSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;

    private HeartEyesOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HeartEyesComponent, ComponentInit>(OnHeartEyesInit);
        SubscribeLocalEvent<HeartEyesComponent, ComponentShutdown>(OnHeartEyesShutdown);

        _overlay = new();
    }

    private void OnHeartEyesInit(EntityUid uid, HeartEyesComponent component, ComponentInit args)
    {
        _overlayMan.AddOverlay(_overlay);
    }

    private void OnHeartEyesShutdown(EntityUid uid, HeartEyesComponent component, ComponentShutdown args)
    {
        if (Count<HeartEyesComponent>() <= 1)
            _overlayMan.RemoveOverlay(_overlay);
    }
}
