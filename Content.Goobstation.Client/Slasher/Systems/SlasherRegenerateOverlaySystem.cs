using Content.Goobstation.Client.Slasher.Overlays;
using Content.Goobstation.Shared.Slasher;
using Content.Goobstation.Shared.Slasher.Components;
using Content.Shared._DV.CCVars;
using Robust.Client.Graphics;
using Robust.Shared.Configuration;

namespace Content.Goobstation.Client.Slasher.Systems;

/// <summary>
/// Registers the SlasherRegenerateOverlay while any slasher has the regenerate ability.
/// </summary>
public sealed class SlasherRegenerateOverlaySystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private SlasherRegenerateOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlasherRegenerateComponent, ComponentInit>(OnRegenerateInit);
        SubscribeLocalEvent<SlasherRegenerateComponent, ComponentShutdown>(OnRegenerateShutdown);

        SubscribeNetworkEvent<SlasherRegenerateEffectEvent>(OnRegenerateEffect);

        Subs.CVar(_cfg, DCCVars.NoVisionFilters, OnNoVisionFiltersChanged);

        _overlay = new();
    }

    private void OnRegenerateInit(EntityUid uid, SlasherRegenerateComponent component, ComponentInit args)
    {
        if (!_cfg.GetCVar(DCCVars.NoVisionFilters))
            _overlayMan.AddOverlay(_overlay);
    }

    private void OnRegenerateShutdown(EntityUid uid, SlasherRegenerateComponent component, ComponentShutdown args)
    {
        if (Count<SlasherRegenerateComponent>() <= 1)
            _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnRegenerateEffect(SlasherRegenerateEffectEvent ev)
    {
        if (_cfg.GetCVar(DCCVars.NoVisionFilters))
            return;

        _overlay.Trigger();
    }

    private void OnNoVisionFiltersChanged(bool enabled)
    {
        if (enabled)
            _overlayMan.RemoveOverlay(_overlay);
        else if (Count<SlasherRegenerateComponent>() > 0)
            _overlayMan.AddOverlay(_overlay);
    }
}
