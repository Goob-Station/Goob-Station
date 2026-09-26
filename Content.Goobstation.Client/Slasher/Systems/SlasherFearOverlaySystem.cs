using Content.Goobstation.Client.Slasher.Overlays;
using Content.Goobstation.Shared.Slasher.Components;
using Content.Shared._DV.CCVars;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Player;

namespace Content.Goobstation.Client.Slasher.Systems;

/// <summary>
/// Drives the default fear grade's intensity from the local victim's fear.
/// </summary>
public sealed class SlasherFearOverlaySystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    private SlasherFearOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlasherFearOverlayComponent, ComponentInit>(OnFearOverlayInit);
        SubscribeLocalEvent<SlasherFearOverlayComponent, ComponentShutdown>(OnFearOverlayShutdown);
        SubscribeLocalEvent<SlasherFearOverlayComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<SlasherFearOverlayComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

        Subs.CVar(_cfg, DCCVars.NoVisionFilters, OnNoVisionFiltersChanged);

        _overlay = new();
    }

    private void OnFearOverlayInit(EntityUid uid, SlasherFearOverlayComponent component, ComponentInit args)
    {
        if (uid == _player.LocalEntity && !_cfg.GetCVar(DCCVars.NoVisionFilters))
            _overlayMan.AddOverlay(_overlay);
    }

    private void OnFearOverlayShutdown(EntityUid uid, SlasherFearOverlayComponent component, ComponentShutdown args)
    {
        if (uid == _player.LocalEntity)
            _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnPlayerAttached(EntityUid uid, SlasherFearOverlayComponent component, LocalPlayerAttachedEvent args)
    {
        if (!_cfg.GetCVar(DCCVars.NoVisionFilters))
            _overlayMan.AddOverlay(_overlay);
    }

    private void OnPlayerDetached(EntityUid uid, SlasherFearOverlayComponent component, LocalPlayerDetachedEvent args)
    {
        _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnNoVisionFiltersChanged(bool enabled)
    {
        if (enabled)
            _overlayMan.RemoveOverlay(_overlay);
        else if (HasComp<SlasherFearOverlayComponent>(_player.LocalEntity))
            _overlayMan.AddOverlay(_overlay);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!TryComp<SlasherFearOverlayComponent>(_player.LocalEntity, out var overlay))
        {
            _overlay.Intensity = 0f;
            return;
        }

        var target = TryComp<SlasherVictimFearBuildupComponent>(_player.LocalEntity, out var feared)
            ? overlay.BaseIntensity + Math.Clamp(feared.Fear, 0f, 1f) * (1f - overlay.BaseIntensity)
            : 0f;

        var dt = frameTime * overlay.FadeSpeed;
        _overlay.Intensity = MathF.Abs(target - _overlay.Intensity) < dt
            ? target
            : _overlay.Intensity + Math.Sign(target - _overlay.Intensity) * dt;
    }
}
