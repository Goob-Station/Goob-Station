using Content.Goobstation.Client.Slasher.Overlays;
using Content.Goobstation.Shared.Slasher.Components;
using Content.Shared._DV.CCVars;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Configuration;

namespace Content.Goobstation.Client.Slasher.Systems;

/// <summary>
/// Plays the SlasherRegenerateOverlay when a nearby slasher regenerates from death.
/// </summary>
public sealed class SlasherRegenerateOverlaySystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    private SlasherRegenerateOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlasherRegenerateOverlayComponent, ComponentStartup>(OnOverlayStartup);
        SubscribeLocalEvent<SlasherRegenerateOverlayComponent, ComponentShutdown>(OnOverlayShutdown);

        Subs.CVar(_cfg, DCCVars.NoVisionFilters, OnNoVisionFiltersChanged);

        _overlay = new();
    }

    private void OnOverlayStartup(EntityUid uid, SlasherRegenerateOverlayComponent component, ComponentStartup args)
    {
        if (uid != _player.LocalEntity || _cfg.GetCVar(DCCVars.NoVisionFilters))
            return;

        _overlayMan.AddOverlay(_overlay);
        _overlay.Trigger();
    }

    private void OnOverlayShutdown(EntityUid uid, SlasherRegenerateOverlayComponent component, ComponentShutdown args)
    {
        if (uid == _player.LocalEntity)
            _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnNoVisionFiltersChanged(bool enabled)
    {
        if (enabled)
            _overlayMan.RemoveOverlay(_overlay);
    }
}
