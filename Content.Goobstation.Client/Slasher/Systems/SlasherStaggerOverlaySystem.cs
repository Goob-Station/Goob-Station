using Content.Goobstation.Client.Slasher.Overlays;
using Content.Goobstation.Shared.Slasher.Components;
using Content.Shared._DV.CCVars;
using Robust.Client.Graphics;
using Robust.Shared.Configuration;

namespace Content.Goobstation.Client.Slasher.Systems;

/// <summary>
/// Registers the SlasherStaggerOverlay while any stagger shockwave is active.
/// </summary>
public sealed class SlasherStaggerOverlaySystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private SlasherStaggerOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlasherStaggerOverlayComponent, ComponentInit>(OnStaggerInit);
        SubscribeLocalEvent<SlasherStaggerOverlayComponent, ComponentShutdown>(OnStaggerShutdown);

        Subs.CVar(_cfg, DCCVars.NoVisionFilters, OnNoVisionFiltersChanged);

        _overlay = new();
    }

    private void OnStaggerInit(EntityUid uid, SlasherStaggerOverlayComponent component, ComponentInit args)
    {
        if (!_cfg.GetCVar(DCCVars.NoVisionFilters))
            _overlayMan.AddOverlay(_overlay);
    }

    private void OnStaggerShutdown(EntityUid uid, SlasherStaggerOverlayComponent component, ComponentShutdown args)
    {
        if (Count<SlasherStaggerOverlayComponent>() <= 1)
            _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnNoVisionFiltersChanged(bool enabled)
    {
        if (enabled)
            _overlayMan.RemoveOverlay(_overlay);
        else if (Count<SlasherStaggerOverlayComponent>() > 0)
            _overlayMan.AddOverlay(_overlay);
    }
}
