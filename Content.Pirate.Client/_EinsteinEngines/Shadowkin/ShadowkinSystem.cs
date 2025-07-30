using Content.Shared.Shadowkin;
using Content.Shared.CCVar;
using Robust.Client.Graphics;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Content.Shared.Humanoid;
using Content.Shared.Abilities.Psionics;
using Content.Client.Overlays;

namespace Content.Client.Shadowkin;

public sealed partial class ShadowkinSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly ISharedPlayerManager _playerMan = default!;

    private ColorTintOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowkinComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<ShadowkinComponent, ComponentShutdown>(Onhutdown);
        SubscribeLocalEvent<ShadowkinComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<ShadowkinComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

        Subs.CVar(_cfg, CCVars.NoVisionFilters, OnNoVisionFiltersChanged);

        _overlay = new();
    }

    private void OnInit(EntityUid uid, ShadowkinComponent component, ComponentInit args)
    {
        if (uid != _playerMan.LocalEntity
            || _cfg.GetCVar(CCVars.NoVisionFilters))
            return;

        _overlayMan.AddOverlay(_overlay);
    }

    private void Onhutdown(EntityUid uid, ShadowkinComponent component, ComponentShutdown args)
    {
        if (uid != _playerMan.LocalEntity)
            return;

        _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnPlayerAttached(EntityUid uid, ShadowkinComponent component, LocalPlayerAttachedEvent args)
    {
        if (_cfg.GetCVar(CCVars.NoVisionFilters))
            return;
            
        _overlayMan.AddOverlay(_overlay);
    }

    private void OnPlayerDetached(EntityUid uid, ShadowkinComponent component, LocalPlayerDetachedEvent args)
    {
        _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnNoVisionFiltersChanged(bool enabled)
    {
        if (enabled)
            _overlayMan.RemoveOverlay(_overlay);
        else
            _overlayMan.AddOverlay(_overlay);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_cfg.GetCVar(CCVars.NoVisionFilters))
            return;

        var uid = _playerMan.LocalEntity;
        if (uid == null
            || !TryComp<ShadowkinComponent>(uid, out var comp)
            || !TryComp<HumanoidAppearanceComponent>(uid, out var humanoid))
            return;

        // 1/3 = 0.333...
        // intensity = min + (power / max)
        // intensity = intensity / 0.333
        // intensity = clamp intensity min, max

        var tintIntensity = 0.65f;
        UpdateShader(new Vector3(humanoid.EyeColor.R, humanoid.EyeColor.G, humanoid.EyeColor.B), tintIntensity);
    }

    private void UpdateShader(Vector3? color, float? intensity)
    {
        while (_overlayMan.HasOverlay<ColorTintOverlay>())
            _overlayMan.RemoveOverlay(_overlay);

        if (color != null)
            _overlay.TintColor = color;
        if (intensity != null)
            _overlay.TintAmount = intensity;

        if (!_cfg.GetCVar(CCVars.NoVisionFilters))
            _overlayMan.AddOverlay(_overlay);
    }
}
