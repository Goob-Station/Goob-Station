using Content.Shared._White.Overlays;
using Content.Shared.GameTicking;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Client._White.Overlays;

public sealed class ThermalVisionSystem : SwitchableOverlaySystem<ThermalVisionComponent, ToggleThermalVisionEvent>
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IOverlayManager _overlayMan = default!;

    private ThermalVisionOverlay _thermalOverlay = default!;
    private BaseSwitchableOverlay<ThermalVisionComponent> _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ThermalVisionComponent, PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<ThermalVisionComponent, PlayerDetachedEvent>(OnPlayerDetached);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRestart);

        _thermalOverlay = new ThermalVisionOverlay();
        _overlay = new BaseSwitchableOverlay<ThermalVisionComponent>();
    }

    private void OnPlayerAttached(EntityUid uid, ThermalVisionComponent component, PlayerAttachedEvent args)
    {
        if (!component.IsActive)
            return;

        UpdateVision(args.Player, component.IsActive);
    }

    private void OnPlayerDetached(EntityUid uid, ThermalVisionComponent component, PlayerDetachedEvent args)
    {
        UpdateVision(args.Player, false);
    }

    private void OnRestart(RoundRestartCleanupEvent ev)
    {
        _overlayMan.RemoveOverlay(_thermalOverlay);
        _overlayMan.RemoveOverlay(_overlay);
    }

    protected override void UpdateVision(EntityUid uid, bool active)
    {
        if (_player.LocalSession?.AttachedEntity != uid)
            return;

        UpdateOverlay(active, _thermalOverlay);
        UpdateOverlay(active, _overlay);
    }

    private void UpdateVision(ICommonSession player, bool active)
    {
        if (_player.LocalSession != player)
            return;

        UpdateOverlay(active, _thermalOverlay);
        UpdateOverlay(active, _overlay);
    }

    private void UpdateOverlay(bool active, Overlay overlay)
    {
        if (_player.LocalEntity == null)
        {
            _overlayMan.RemoveOverlay(overlay);
            return;
        }

        active |= TryComp<ThermalVisionComponent>(_player.LocalEntity.Value, out var component) && component.IsActive;

        if (active)
            _overlayMan.AddOverlay(overlay);
        else
            _overlayMan.RemoveOverlay(overlay);
    }
}
