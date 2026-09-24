using Content.Goobstation.Client.Slasher.Overlays;
using Content.Goobstation.Shared.Slasher.Components;
using Content.Shared._DV.CCVars;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Player;

namespace Content.Goobstation.Client.Slasher.Systems;

public sealed class SlasherValentineOverlaySystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    private SlasherValentineOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlasherValentineOverlayComponent, ComponentInit>(OnValentineOverlayInit);
        SubscribeLocalEvent<SlasherValentineOverlayComponent, ComponentShutdown>(OnValentineOverlayShutdown);
        SubscribeLocalEvent<SlasherValentineOverlayComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<SlasherValentineOverlayComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

        Subs.CVar(_cfg, DCCVars.NoVisionFilters, OnNoVisionFiltersChanged);

        _overlay = new();
    }

    private void OnValentineOverlayInit(EntityUid uid, SlasherValentineOverlayComponent component, ComponentInit args)
    {
        if (uid == _player.LocalEntity && !_cfg.GetCVar(DCCVars.NoVisionFilters))
            _overlayMan.AddOverlay(_overlay);
    }

    private void OnValentineOverlayShutdown(EntityUid uid, SlasherValentineOverlayComponent component, ComponentShutdown args)
    {
        if (uid == _player.LocalEntity)
            _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnPlayerAttached(EntityUid uid, SlasherValentineOverlayComponent component, LocalPlayerAttachedEvent args)
    {
        if (!_cfg.GetCVar(DCCVars.NoVisionFilters))
            _overlayMan.AddOverlay(_overlay);
    }

    private void OnPlayerDetached(EntityUid uid, SlasherValentineOverlayComponent component, LocalPlayerDetachedEvent args)
    {
        _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnNoVisionFiltersChanged(bool enabled)
    {
        if (enabled)
            _overlayMan.RemoveOverlay(_overlay);
        else if (HasComp<SlasherValentineOverlayComponent>(_player.LocalEntity))
            _overlayMan.AddOverlay(_overlay);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!TryComp<SlasherValentineOverlayComponent>(_player.LocalEntity, out var overlay))
        {
            _overlay.Intensity = 0f;
            return;
        }

        var target = TryComp<FearedComponent>(_player.LocalEntity, out var feared)
            ? overlay.BaseIntensity + Math.Clamp(feared.Fear, 0f, 1f) * (1f - overlay.BaseIntensity)
            : 0f;

        var dt = frameTime * overlay.FadeSpeed;
        _overlay.Intensity = MathF.Abs(target - _overlay.Intensity) < dt
            ? target
            : _overlay.Intensity + Math.Sign(target - _overlay.Intensity) * dt;
    }
}
