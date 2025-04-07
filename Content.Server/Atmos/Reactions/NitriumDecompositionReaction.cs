using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Reactions;
using JetBrains.Annotations;

namespace Content.Server.Atmos.Reactions;

/// <summary>
///     Assmos - /tg/ gases
///     The decomposition of nitrium in the presence of oxygen at temperatures below 343K.
/// </summary>
[UsedImplicitly]
public sealed partial class NitriumDecompositionReaction : IGasReactionEffect
{
    public ReactionResult React(GasMixture mixture, IGasMixtureHolder? holder, AtmosphereSystem atmosphereSystem, float heatScale)
    {
        var initNitrium = mixture.GetMoles(Gas.Nitrium);
        var initOxygen = mixture.GetMoles(Gas.Oxygen);

        if (mixture.Temperature > Atmospherics.T0C + 70)
            return ReactionResult.NoReaction;

        var efficiency = Math.Min(mixture.Temperature / 2984f, initNitrium);

        var nitriumRemoved = efficiency;
        var waterVaporProduced = efficiency;
        var nitrogenProduced = efficiency;

        if (efficiency <= 0 || initNitrium - nitriumRemoved < 0)
            return ReactionResult.NoReaction;

        mixture.AdjustMoles(Gas.Nitrium, -nitriumRemoved);
        mixture.AdjustMoles(Gas.WaterVapor, waterVaporProduced);
        mixture.AdjustMoles(Gas.Nitrogen, nitrogenProduced);

        var energyReleased = efficiency * Atmospherics.NitriumDecompositionEnergy;
        var heatCap = atmosphereSystem.GetHeatCapacity(mixture, true);
        if (heatCap > Atmospherics.MinimumHeatCapacity)
            mixture.Temperature = Math.Max((mixture.Temperature * heatCap + energyReleased) / heatCap, Atmospherics.TCMB);

        return ReactionResult.Reacting;
    }
}
