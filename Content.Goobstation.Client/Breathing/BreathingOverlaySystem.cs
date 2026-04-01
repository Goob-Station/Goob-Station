using Content.Goobstation.Client.Breathing.Overlays;
using Content.Goobstation.Shared.Breathing;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Goobstation.Client.Breathing;

public sealed class BreathingOverlaySystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IOverlayManager _overlayMan = default!;

    private BreathBlurOverlay _blurOverlay = default!;
    private BreathIndicatorOverlay _indicatorOverlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ManualBreathingComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<ManualBreathingComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<ManualBreathingComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<ManualBreathingComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

        _blurOverlay = new BreathBlurOverlay();
        _indicatorOverlay = new BreathIndicatorOverlay();
    }

    private void OnInit(EntityUid uid, ManualBreathingComponent comp, ComponentInit args)
    {
        if (_playerManager.LocalEntity == uid)
            AddOverlays();
    }

    private void OnShutdown(EntityUid uid, ManualBreathingComponent comp, ComponentShutdown args)
    {
        if (_playerManager.LocalEntity == uid)
            RemoveOverlays();
    }

    private void OnPlayerAttached(EntityUid uid, ManualBreathingComponent comp, LocalPlayerAttachedEvent args)
    {
        AddOverlays();
    }

    private void OnPlayerDetached(EntityUid uid, ManualBreathingComponent comp, LocalPlayerDetachedEvent args)
    {
        RemoveOverlays();
    }

    private void AddOverlays()
    {
        _overlayMan.AddOverlay(_blurOverlay);
        _overlayMan.AddOverlay(_indicatorOverlay);
    }

    private void RemoveOverlays()
    {
        _overlayMan.RemoveOverlay(_blurOverlay);
        _overlayMan.RemoveOverlay(_indicatorOverlay);
    }
}
