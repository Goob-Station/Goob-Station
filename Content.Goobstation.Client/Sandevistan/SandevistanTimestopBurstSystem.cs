using Content.Goobstation.Shared.Sandevistan;
using Content.Shared._DV.CCVars;
using Robust.Client.Graphics;
using Robust.Shared.Configuration;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Sandevistan;

/// <summary>
/// Plays the timestop swirl effect out of this entity for everyone who can see it.
/// </summary>
public sealed partial class SandevistanTimestopBurstSystem : EntitySystem
{
    [Dependency] private IOverlayManager _overlayMan = default!;
    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private IGameTiming _timing = default!;

    private readonly SandevistanTimestopBurstOverlay _overlay = new();

    public readonly Dictionary<EntityUid, SandevistanTimestopBurstComponent> Reversing = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SandevistanTimestopBurstComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<SandevistanTimestopBurstComponent, ComponentShutdown>(OnShutdown);

        Subs.CVar(_cfg, DCCVars.NoVisionFilters, OnNoVisionFiltersChanged);
    }

    private void OnInit(EntityUid uid, SandevistanTimestopBurstComponent component, ComponentInit args)
    {
        component.StartedAt = _timing.RealTime;

        if (!_cfg.GetCVar(DCCVars.NoVisionFilters))
            _overlayMan.AddOverlay(_overlay);
    }

    private void OnShutdown(EntityUid uid, SandevistanTimestopBurstComponent component, ComponentShutdown args)
    {
        component.ReversedAt = _timing.RealTime;
        Reversing[uid] = component;
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        var ended = false;
        foreach (var (uid, burst) in Reversing)
        {
            if (_timing.RealTime - burst.ReversedAt < burst.Lasts)
                continue;

            Reversing.Remove(uid);
            ended = true;
        }

        if (ended && Reversing.Count == 0 && Count<SandevistanTimestopBurstComponent>() == 0)
            _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnNoVisionFiltersChanged(bool enabled)
    {
        if (enabled)
            _overlayMan.RemoveOverlay(_overlay);
        else if (Count<SandevistanTimestopBurstComponent>() > 0 || Reversing.Count > 0)
            _overlayMan.AddOverlay(_overlay);
    }
}
