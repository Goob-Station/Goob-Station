using Content.Goobstation.Shared.Sandevistan;
using Content.Shared._DV.CCVars;
using Robust.Client.Graphics;
using Robust.Shared.Configuration;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Sandevistan;

/// <summary>
/// Plays the timestop swirl effect out of this entity for everyone who can see it.
/// </summary>
public sealed class SandevistanTimestopBurstSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private SandevistanTimestopBurstOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SandevistanTimestopBurstComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<SandevistanTimestopBurstComponent, ComponentShutdown>(OnShutdown);

        Subs.CVar(_cfg, DCCVars.NoVisionFilters, OnNoVisionFiltersChanged);

        _overlay = new();
    }

    private void OnInit(EntityUid uid, SandevistanTimestopBurstComponent component, ComponentInit args)
    {
        component.StartedAt = _timing.RealTime;

        if (!_cfg.GetCVar(DCCVars.NoVisionFilters))
            _overlayMan.AddOverlay(_overlay);
    }

    private void OnShutdown(EntityUid uid, SandevistanTimestopBurstComponent component, ComponentShutdown args)
    {
        if (Count<SandevistanTimestopBurstComponent>() <= 1)
            _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnNoVisionFiltersChanged(bool enabled)
    {
        if (enabled)
            _overlayMan.RemoveOverlay(_overlay);
        else if (Count<SandevistanTimestopBurstComponent>() > 0)
            _overlayMan.AddOverlay(_overlay);
    }
}
