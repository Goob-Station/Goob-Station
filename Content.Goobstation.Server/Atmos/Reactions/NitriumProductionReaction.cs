using Content.Server.Atmos;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Reactions;
using JetBrains.Annotations;

namespace Content.Goobstation.Server.Atmos.Reactions;

/// <summary>
///     Assmos - /tg/ gases
///     Produces Nitrium by mixing Tritium, Nitrogen and BZ at temperatures above 1500K. 
/// </summary>
[UsedImplicitly]
public sealed partial class NitriumProductionReaction : IGasReactionEffect
{
    public ReactionResult React(GasMixture mixture, IGasMixtureHolder? holder, AtmosphereSystem atmosphereSystem, float heatScale)
    {
        if (mixture.Temperature > 20f && mixture.GetMoles(Gas.HyperNoblium) >= 5f)
            return ReactionResult.NoReaction;

        var initTritium = mixture.GetMoles(Gas.Tritium);
        var initNitrogen = mixture.GetMoles(Gas.Nitrogen);
        var initBZ = mixture.GetMoles(Gas.BZ);

        var efficiency = Math.Min(mixture.Temperature / 2984f, Math.Min(initBZ * 20f, Math.Min(initTritium, initNitrogen)));

        var tritiumRemoved = efficiency;
        var nitrogenRemoved = efficiency;
        var bzRemoved = efficiency * 0.05f;
        var nitriumProduced = efficiency;

        if (efficiency <= 0 || initTritium - tritiumRemoved < 0 || initNitrogen - nitrogenRemoved < 0 || initBZ - bzRemoved < 0)
            return ReactionResult.NoReaction;

        mixture.AdjustMoles(Gas.Tritium, -tritiumRemoved);
        mixture.AdjustMoles(Gas.Nitrogen, -nitrogenRemoved);
        mixture.AdjustMoles(Gas.BZ, -bzRemoved);
        mixture.AdjustMoles(Gas.Nitrium, nitriumProduced);

        var energyReleased = efficiency * Atmospherics.NitriumProductionEnergy;
        var heatCap = atmosphereSystem.GetHeatCapacity(mixture, true);
        if (heatCap > Atmospherics.MinimumHeatCapacity)
            mixture.Temperature = Math.Max((mixture.Temperature * heatCap + energyReleased) / heatCap, Atmospherics.TCMB);

        return ReactionResult.Reacting;
    }
}
