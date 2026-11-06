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
    [Dependency] private IOverlayManager _overlayMan = default!;
    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private IPrototypeManager _proto = default!;

    private readonly CinematicPressureOverlay _overlay = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CinematicPressureComponent, ComponentInit>(OnPressureInit);
        SubscribeLocalEvent<CinematicPressureComponent, ComponentShutdown>(OnPressureShutdown);
        SubscribeLocalEvent<CinematicPressureComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<CinematicPressureComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);
        SubscribeLocalEvent<CinematicPressureComponent, CinematicUpdatedEvent>(OnCinematicUpdated);

        Subs.CVar(_cfg, DCCVars.NoVisionFilters, OnNoVisionFiltersChanged);
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

    private void OnCinematicUpdated(Entity<CinematicPressureComponent> ent, ref CinematicUpdatedEvent args)
    {
        ent.Comp.Strength = args.Strength;
        ent.Comp.Remaining = args.Remaining;
    }

    private void OnPressureInit(Entity<CinematicPressureComponent> ent, ref ComponentInit args)
    {
        SetShader(ent.Comp);

        if (!_cfg.GetCVar(DCCVars.NoVisionFilters))
            _overlayMan.AddOverlay(_overlay);
    }

    private void SetShader(CinematicPressureComponent component)
    {
        _overlay.Shader?.Dispose();
        _overlay.Shader = _proto.Index<ShaderPrototype>(component.Shader).InstanceUnique();
    }

    private void OnPressureShutdown(Entity<CinematicPressureComponent> ent, ref ComponentShutdown args)
    {
        var query = EntityQueryEnumerator<CinematicPressureComponent>();
        while (query.MoveNext(out var other, out _))
        {
            if (other != ent.Owner)
                return;
        }

        _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnPlayerAttached(Entity<CinematicPressureComponent> ent, ref LocalPlayerAttachedEvent args)
    {
        SetShader(ent.Comp);

        if (!_cfg.GetCVar(DCCVars.NoVisionFilters))
            _overlayMan.AddOverlay(_overlay);
    }

    private void OnPlayerDetached(Entity<CinematicPressureComponent> ent, ref LocalPlayerDetachedEvent args)
        => _overlayMan.RemoveOverlay(_overlay);

    private void OnNoVisionFiltersChanged(bool enabled)
    {
        if (enabled)
            _overlayMan.RemoveOverlay(_overlay);
        else
            _overlayMan.AddOverlay(_overlay);
    }
}
