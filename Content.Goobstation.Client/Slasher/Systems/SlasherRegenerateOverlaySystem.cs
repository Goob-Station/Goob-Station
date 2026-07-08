using Content.Goobstation.Client.Slasher.Overlays;
using Content.Goobstation.Shared.Slasher;
using Robust.Client.Graphics;

namespace Content.Goobstation.Client.Slasher.Systems;

/// <summary>
/// Registers the SlasherRegenerateOverlay.
/// </summary>
public sealed class SlasherRegenerateOverlaySystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;

    private SlasherRegenerateOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        _overlay = new();
        _overlayMan.AddOverlay(_overlay);

        SubscribeNetworkEvent<SlasherRegenerateEffectEvent>(OnRegenerateEffect);
    }

    public override void Shutdown()
    {
        base.Shutdown();

        _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnRegenerateEffect(SlasherRegenerateEffectEvent ev)
    {
        _overlay.Trigger();
    }
}
