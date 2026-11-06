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

    public readonly Dictionary<EntityUid, SandevistanTimestopBurstComponent> Bursts = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SandevistanTimestopBurstComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<SandevistanTimestopBurstComponent, ComponentShutdown>(OnShutdown);

        Subs.CVar(_cfg, DCCVars.NoVisionFilters, OnNoVisionFiltersChanged);
    }

    private void OnInit(EntityUid uid, SandevistanTimestopBurstComponent component, ComponentInit args)
    {
        var now = _timing.RealTime;

        component.StartedAt = Bursts.TryGetValue(uid, out var previous)
            ? now - component.Lasts * Progress(previous, now)
            : now;
        Bursts[uid] = component;

        if (!_cfg.GetCVar(DCCVars.NoVisionFilters))
            _overlayMan.AddOverlay(_overlay);
    }

    private void OnShutdown(EntityUid uid, SandevistanTimestopBurstComponent component, ComponentShutdown args)
    {
        component.ReversedAt = _timing.RealTime;
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        var now = _timing.RealTime;
        var ended = false;
        foreach (var (uid, burst) in Bursts)
        {
            if (burst.ReversedAt == null || Progress(burst, now) > 0f)
                continue;

            Bursts.Remove(uid);
            ended = true;
        }

        if (ended && Bursts.Count == 0)
            _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnNoVisionFiltersChanged(bool enabled)
    {
        if (enabled)
            _overlayMan.RemoveOverlay(_overlay);
        else if (Bursts.Count > 0)
            _overlayMan.AddOverlay(_overlay);
    }

    public static float Progress(SandevistanTimestopBurstComponent burst, TimeSpan now)
    {
        var lasts = burst.Lasts.TotalSeconds;
        var forward = Math.Min(((burst.ReversedAt ?? now) - burst.StartedAt).TotalSeconds / lasts, 1.0);
        if (burst.ReversedAt is not { } reversedAt)
            return (float) forward;

        return (float) (forward - (now - reversedAt).TotalSeconds / lasts);
    }
}
