using Content.Goobstation.Client.Slasher.Overlays;
using Content.Goobstation.Shared.Slasher.Components;
using Robust.Client.Graphics;

namespace Content.Goobstation.Client.Slasher.Systems;

/// <summary>
/// Adds star eyes over the users eyes.
/// </summary>
public sealed class MagicalStarEyesSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;

    private MagicalStarEyesOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MagicalStarEyesComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<MagicalStarEyesComponent, ComponentShutdown>(OnShutdown);

        _overlay = new();
    }

    private void OnInit(Entity<MagicalStarEyesComponent> ent, ref ComponentInit args)
    {
        _overlayMan.AddOverlay(_overlay);
    }

    private void OnShutdown(Entity<MagicalStarEyesComponent> ent, ref ComponentShutdown args)
    {
        if (Count<MagicalStarEyesComponent>() <= 1)
            _overlayMan.RemoveOverlay(_overlay);
    }
}
