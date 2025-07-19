using Content.Server.Anomaly.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.Research.Components;
using Content.Shared.Anomaly.Components;
using Content.Shared.Mobs;
using Content.Shared.Psionics.Glimmer;
using Content.Shared.Power;

namespace Content.Server.Psionics.Glimmer;

/// <summary>
/// Handles structures which add/subtract glimmer.
/// </summary>
public sealed class GlimmerStructuresSystem : EntitySystem
{
    [Dependency] private readonly PowerReceiverSystem _powerReceiverSystem = default!;
    [Dependency] private readonly GlimmerSystem _glimmerSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AnomalyVesselComponent, PowerChangedEvent>(OnAnomalyVesselPowerChanged);

        SubscribeLocalEvent<GlimmerSourceComponent, AnomalyPulseEvent>(OnAnomalyPulse);
        SubscribeLocalEvent<GlimmerSourceComponent, AnomalySupercriticalEvent>(OnAnomalySupercritical);
        SubscribeLocalEvent<GlimmerSourceComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<GlimmerSourceComponent, ComponentStartup>(OnInit);
    }

    private void OnInit(EntityUid uid, GlimmerSourceComponent component, ComponentStartup args)
    {
        if (component.ResearchPointGeneration != null)
        {
            EnsureComp<ResearchPointSourceComponent>(uid, out var points);
            points.PointsPerSecond = component.ResearchPointGeneration.Value;
            points.Active = true;
        }
    }

    private void OnAnomalyVesselPowerChanged(EntityUid uid, AnomalyVesselComponent component, ref PowerChangedEvent args)
    {
        if (TryComp<GlimmerSourceComponent>(component.Anomaly, out var glimmerSource))
            glimmerSource.Active = args.Powered;
    }

    private void OnAnomalyPulse(EntityUid uid, GlimmerSourceComponent component, ref AnomalyPulseEvent args)
    {
        // Anomalies are meant to have GlimmerSource on them with the
        // active flag set to false, as they will be set to actively
        // generate glimmer when scanned to an anomaly vessel for
        // harvesting research points.
        //
        // It is not a bug that glimmer increases on pulse or
        // supercritical with an inactive glimmer source.
        //
        // However, this will need to be reworked if a distinction
        // needs to be made in the future. I suggest a GlimmerAnomaly
        // component.

        if (TryComp<AnomalyComponent>(uid, out var anomaly))
            _glimmerSystem.DeltaGlimmerOutput(5f * anomaly.Severity);
    }

    private void OnAnomalySupercritical(EntityUid uid, GlimmerSourceComponent component, ref AnomalySupercriticalEvent args)
    {
        _glimmerSystem.DeltaGlimmerOutput(100);
    }

    private void OnMobStateChanged(EntityUid uid, GlimmerSourceComponent component, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Alive)
            component.Active = false;
    }

    public override void Update(float frameTime)
    {
        if (!_glimmerSystem.GetGlimmerEnabled())
            return;

        base.Update(frameTime);

        var totalSources = Count<GlimmerSourceComponent>();
        var glimmerSources = EntityQueryEnumerator<GlimmerSourceComponent>();
        while (glimmerSources.MoveNext(out var uid, out var component))
        {
            if (!component.Active
                || component.RequiresPower && !_powerReceiverSystem.IsPowered(uid))
                continue;

            if (component.ResearchPointGeneration != null
            && TryComp(uid, out ResearchPointSourceComponent? research))
                research.PointsPerSecond = (int) Math.Round(
                    component.ResearchPointGeneration.Value
                    / (MathF.Log(totalSources, 4) + 1)
                    * _glimmerSystem.GetGlimmerEquilibriumRatio());

            _glimmerSystem.DeltaGlimmerInput(component.GlimmerPerSecond * frameTime);
        }
    }
}
