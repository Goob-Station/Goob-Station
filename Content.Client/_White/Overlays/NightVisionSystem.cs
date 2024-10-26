using Content.Shared._White.Overlays;
using Content.Shared.GameTicking;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Client._White.Overlays;

public sealed class NightVisionSystem : SwitchableOverlaySystem<NightVisionComponent, ToggleNightVisionEvent>
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly ILightManager _lightManager = default!;

    private BaseSwitchableOverlay<NightVisionComponent> _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NightVisionComponent, PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<NightVisionComponent, PlayerDetachedEvent>(OnPlayerDetached);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRestart);

        _overlay = new BaseSwitchableOverlay<NightVisionComponent>();
    }

    private void OnPlayerAttached(EntityUid uid, NightVisionComponent component, PlayerAttachedEvent args)
    {
        if (!component.IsActive)
            return;

        UpdateVision(args.Player, component.IsActive);
    }

    private void OnPlayerDetached(EntityUid uid, NightVisionComponent component, PlayerDetachedEvent args)
    {
        UpdateVision(args.Player, false);
    }

    private void OnRestart(RoundRestartCleanupEvent ev)
    {
        _overlayMan.RemoveOverlay(_overlay);
        _lightManager.DrawLighting = true;
    }

    protected override void UpdateVision(EntityUid uid, bool active)
    {
        if (_player.LocalSession?.AttachedEntity != uid)
            return;

        UpdateOverlay(active);
        UpdateNightVision(active);
    }

    private void UpdateVision(ICommonSession player, bool active)
    {
        if (_player.LocalSession != player)
            return;

        UpdateOverlay(active);
        UpdateNightVision(active);
    }

    private void UpdateNightVision(bool active)
    {
        _lightManager.DrawLighting = !active;
    }

    private void UpdateOverlay(bool active)
    {
        if (_player.LocalEntity == null)
        {
            _overlayMan.RemoveOverlay(_overlay);
            return;
        }

        active |= TryComp<NightVisionComponent>(_player.LocalEntity.Value, out var component) && component.IsActive;

        if (active)
            _overlayMan.AddOverlay(_overlay);
        else
            _overlayMan.RemoveOverlay(_overlay);
    }
}
