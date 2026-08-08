using Content.Goobstation.Client.Slasher.Overlays;
using Content.Goobstation.Shared.Slasher.Components;
using Robust.Client.Graphics;

namespace Content.Goobstation.Client.Slasher.Systems;

/// <summary>
/// Registers the LovestruckProgressOverlay while any entity is being charmed.
/// </summary>
public sealed class LovestruckProgressOverlaySystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;

    private LovestruckProgressOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LovestruckComponent, ComponentInit>(OnLovestruckInit);
        SubscribeLocalEvent<LovestruckComponent, ComponentShutdown>(OnLovestruckShutdown);

        _overlay = new();
    }

    private void OnLovestruckInit(EntityUid uid, LovestruckComponent component, ComponentInit args)
    {
        _overlayMan.AddOverlay(_overlay);
    }

    private void OnLovestruckShutdown(EntityUid uid, LovestruckComponent component, ComponentShutdown args)
    {
        if (Count<LovestruckComponent>() <= 1)
            _overlayMan.RemoveOverlay(_overlay);
    }
}
