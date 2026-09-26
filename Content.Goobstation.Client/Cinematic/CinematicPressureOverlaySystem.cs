using Content.Goobstation.Shared.Cinematic;
using Content.Shared._DV.CCVars;
using Robust.Client.Graphics;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Cinematic;

/// <summary>
/// Drives the pressure aura.
/// </summary>
public sealed partial class CinematicPressureOverlaySystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    private CinematicPressureOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CinematicPressureComponent, ComponentInit>(OnPressureInit);
        SubscribeLocalEvent<CinematicPressureComponent, ComponentShutdown>(OnPressureShutdown);
        SubscribeLocalEvent<CinematicPressureComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<CinematicPressureComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);
        SubscribeLocalEvent<CinematicPressureComponent, CinematicUpdatedEvent>(OnCinematicUpdated);

        Subs.CVar(_cfg, DCCVars.NoVisionFilters, OnNoVisionFiltersChanged);

        _overlay = new();
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        var query = EntityQueryEnumerator<CinematicPressureComponent>();
        while (query.MoveNext(out var pressure))
        {
            var target = pressure.Strength * pressure.Intensity;

            if (pressure.Current < target)
                pressure.Current = MathF.Min(target, pressure.Current + frameTime / MathF.Max(0.01f, pressure.FadeInTime));
            else
                pressure.Current = MathF.Max(target, pressure.Current - frameTime * FadeOutRate(pressure));

            UpdateShockwave(pressure, frameTime);
        }
    }

    private static float FadeOutRate(CinematicPressureComponent pressure)
    {
        var rate = 1f / MathF.Max(0.01f, pressure.FadeOutTime);
        if (pressure.Remaining > 0f)
            rate = MathF.Max(rate, pressure.Current / pressure.Remaining);

        return rate;
    }

    private static void UpdateShockwave(CinematicPressureComponent pressure, float frameTime)
    {
        pressure.Age += frameTime;
        pressure.Shock = -1f;

        if (pressure.ShockDuration <= 0f)
            return;

        var progress = (pressure.Age - pressure.ShockTime) / pressure.ShockDuration;
        if (progress >= 0f && progress <= 1f)
            pressure.Shock = progress;
    }

    private void OnCinematicUpdated(EntityUid uid, CinematicPressureComponent component, ref CinematicUpdatedEvent args)
    {
        component.Strength = args.Strength;
        component.Remaining = args.Remaining;
    }

    private void OnPressureInit(EntityUid uid, CinematicPressureComponent component, ComponentInit args)
    {
        SetShader(component);

        if (!_cfg.GetCVar(DCCVars.NoVisionFilters))
            _overlayMan.AddOverlay(_overlay);
    }

    private void SetShader(CinematicPressureComponent component)
    {
        _overlay.Shader?.Dispose();
        _overlay.Shader = _proto.Index<ShaderPrototype>(component.Shader).InstanceUnique();
    }

    private void OnPressureShutdown(EntityUid uid, CinematicPressureComponent component, ComponentShutdown args)
    {
        var query = EntityQueryEnumerator<CinematicPressureComponent>();
        while (query.MoveNext(out var other, out _))
        {
            if (other != uid)
                return;
        }

        _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnPlayerAttached(EntityUid uid, CinematicPressureComponent component, LocalPlayerAttachedEvent args)
    {
        SetShader(component);

        if (!_cfg.GetCVar(DCCVars.NoVisionFilters))
            _overlayMan.AddOverlay(_overlay);
    }

    private void OnPlayerDetached(EntityUid uid, CinematicPressureComponent component, LocalPlayerDetachedEvent args)
        => _overlayMan.RemoveOverlay(_overlay);

    private void OnNoVisionFiltersChanged(bool enabled)
    {
        if (enabled)
            _overlayMan.RemoveOverlay(_overlay);
        else
            _overlayMan.AddOverlay(_overlay);
    }
}
