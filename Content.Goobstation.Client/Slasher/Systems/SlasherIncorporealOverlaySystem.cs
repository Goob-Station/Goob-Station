using Content.Goobstation.Client.Slasher.Overlays;
using Robust.Client.Graphics;

namespace Content.Goobstation.Client.Slasher.Systems;

/// <summary>
/// Registers the SlasherIncorporealOverlay.
/// </summary>
public sealed class SlasherIncorporealOverlaySystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;

    private SlasherIncorporealOverlay _overlay = default!;

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
