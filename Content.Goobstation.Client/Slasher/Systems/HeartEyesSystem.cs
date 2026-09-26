using Content.Goobstation.Client.Slasher.Overlays;
using Content.Goobstation.Shared.Slasher.Components;
using Robust.Client.Graphics;

namespace Content.Goobstation.Client.Slasher.Systems;

/// <summary>
/// Adds heart eyes on top of the users eyes
/// </summary>
public sealed class HeartEyesSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;

    private HeartEyesOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HeartEyesComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<HeartEyesComponent, ComponentShutdown>(OnShutdown);

        _overlay = new();
    }

    private void OnInit(Entity<HeartEyesComponent> ent, ref ComponentInit args)
    {
        _overlayMan.AddOverlay(_overlay);
    }

    private void OnShutdown(Entity<HeartEyesComponent> ent, ref ComponentShutdown args)
    {
        if (Count<HeartEyesComponent>() <= 1)
            _overlayMan.RemoveOverlay(_overlay);
    }
}
