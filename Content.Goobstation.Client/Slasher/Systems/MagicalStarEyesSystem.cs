using Content.Goobstation.Client.Slasher.Overlays;
using Content.Goobstation.Shared.Slasher.Components;
using Robust.Client.Graphics;

namespace Content.Goobstation.Client.Slasher.Systems;

/// <summary>
/// Registers the MagicalStarEyesOverlay while any entity has star eyes.
/// </summary>
public sealed class MagicalStarEyesSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;

    private MagicalStarEyesOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MagicalStarEyesComponent, ComponentInit>(OnStarEyesInit);
        SubscribeLocalEvent<MagicalStarEyesComponent, ComponentShutdown>(OnStarEyesShutdown);

        _overlay = new();
    }

    private void OnStarEyesInit(EntityUid uid, MagicalStarEyesComponent component, ComponentInit args)
    {
        _overlayMan.AddOverlay(_overlay);
    }

    private void OnStarEyesShutdown(EntityUid uid, MagicalStarEyesComponent component, ComponentShutdown args)
    {
        if (Count<MagicalStarEyesComponent>() <= 1)
            _overlayMan.RemoveOverlay(_overlay);
    }
}
