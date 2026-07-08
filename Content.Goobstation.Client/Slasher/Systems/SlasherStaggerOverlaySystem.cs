using Content.Goobstation.Client.Slasher.Overlays;
using Robust.Client.Graphics;

namespace Content.Goobstation.Client.Slasher.Systems;

/// <summary>
/// Registers the SlasherStaggerOverlay.
/// </summary>
public sealed class SlasherStaggerOverlaySystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;

    private SlasherStaggerOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        _overlay = new();
        _overlayMan.AddOverlay(_overlay);
    }

    public override void Shutdown()
    {
        base.Shutdown();

        _overlayMan.RemoveOverlay(_overlay);
    }
}
