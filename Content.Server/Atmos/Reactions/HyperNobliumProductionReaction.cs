using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Reactions;
using JetBrains.Annotations;

namespace Content.Server.Atmos.Reactions;

[UsedImplicitly]
public sealed partial class HyperNobliumProductionReaction : IGasReactionEffect
{
    public ReactionResult React(GasMixture mixture, IGasMixtureHolder? holder, AtmosphereSystem atmosphereSystem, float heatScale)
    {
        if (mixture.Temperature > 20f && mixture.GetMoles(Gas.HyperNoblium) >= 5f)
            return ReactionResult.NoReaction;
            
        var initN2 = mixture.GetMoles(Gas.Nitrogen);
        var initTrit = mixture.GetMoles(Gas.Tritium);
        var initBZ = mixture.GetMoles(Gas.BZ);

        //reduces trit consumption in presence of bz upward to 0.1% reduction
        var reductionFactor = Math.Clamp(initTrit / (initTrit + initBZ), 0.001f, 1f);

        var nobFormed = Math.Min((initN2 + initTrit) * 0.01f, Math.Min(initTrit * 1f / (5f * reductionFactor), initN2 * 0.1f));
        if (nobFormed <= 0 || (initTrit - 5f) * nobFormed * reductionFactor < 0 || (initN2 - 10f) * nobFormed < 0)
            return ReactionResult.NoReaction;

        mixture.AdjustMoles(Gas.Tritium, -5f * nobFormed * reductionFactor);
        mixture.AdjustMoles(Gas.Nitrogen, -10f * nobFormed);
        mixture.AdjustMoles(Gas.HyperNoblium, nobFormed);

        var energyReleased = nobFormed * (Atmospherics.HyperNobliumProductionEnergy/Math.Max(initBZ, 1));

        var heatCap = atmosphereSystem.GetHeatCapacity(mixture, true);
        if (heatCap > Atmospherics.MinimumHeatCapacity)
            mixture.Temperature = Math.Max((mixture.Temperature * heatCap + energyReleased) / heatCap, Atmospherics.TCMB);

        return ReactionResult.Reacting;
    }
}