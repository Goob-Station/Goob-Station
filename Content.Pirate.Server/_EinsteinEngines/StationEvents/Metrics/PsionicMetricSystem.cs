using Content.Goobstation.Server.StationEvents.Metric;
using Content.Shared.Psionics.Glimmer;

namespace Content.Server.StationEvents.Metrics;

/// <summary>
/// Tracks psionic activity on the station based on glimmer levels. PIRATE SHITCODE
/// </summary>
public sealed class PsionicMetricSystem : EntitySystem
{
    [Dependency] private readonly GlimmerSystem _glimmerSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PsionicMetricComponent, CalculateChaosEvent>(OnCalculateChaos);
    }

    private void OnCalculateChaos(EntityUid uid, PsionicMetricComponent component, ref CalculateChaosEvent args)
    {
        if (!_glimmerSystem.GetGlimmerEnabled())
            return;

        var glimmerOutput = _glimmerSystem.GlimmerOutput;
        var glimmerTier = _glimmerSystem.GetGlimmerTier();

        // Convert glimmer (0-1000) to psionic chaos metric
        // Higher glimmer = more psionic chaos
        var psionicChaos = glimmerOutput / 10.0; // Scale to 0-100 range

        // Add additional chaos based on glimmer tier for more dramatic effects
        switch (glimmerTier)
        {
            case GlimmerTier.Minimal:
                // Very low glimmer - minimal psionic activity
                break;
            case GlimmerTier.Low:
                // Low glimmer - minor boost
                psionicChaos += 5;
                break;
            case GlimmerTier.Moderate:
                // Moderate glimmer - noticeable boost
                psionicChaos += 15;
                break;
            case GlimmerTier.High:
                // High glimmer - significant boost
                psionicChaos += 30;
                break;
            case GlimmerTier.Dangerous:
                // Dangerous glimmer - major boost
                psionicChaos += 50;
                break;
            case GlimmerTier.Critical:
                // Critical glimmer - extreme boost
                psionicChaos += 75;
                break;
        }

        args.Metrics.ChaosDict[ChaosMetric.Psionic] = psionicChaos;
    }
}
