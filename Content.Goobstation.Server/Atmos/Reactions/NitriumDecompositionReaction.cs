using Content.Server.Atmos;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Reactions;
using JetBrains.Annotations;

namespace Content.Goobstation.Server.Atmos.Reactions;

/// <summary>
///     Assmos - /tg/ gases
///     The decomposition of nitrium in the presence of oxygen at temperatures below 343K.
/// </summary>
[UsedImplicitly]
public sealed partial class NitriumDecompositionReaction : IGasReactionEffect
{
    public ReactionResult React(GasMixture mixture, IGasMixtureHolder? holder, AtmosphereSystem atmosphereSystem, float heatScale)
    {
        if (mixture.Temperature > 20f && mixture.GetMoles(Gas.HyperNoblium) >= 5f)
            return ReactionResult.NoReaction;

        var initNitrium = mixture.GetMoles(Gas.Nitrium);
        var initOxygen = mixture.GetMoles(Gas.Oxygen);

        var efficiency = Math.Min(mixture.Temperature / 2984f, initNitrium);

        var nitriumRemoved = efficiency;
        var h2Produced = efficiency;
        var nitrogenProduced = efficiency;

        if (efficiency <= 0 || initNitrium - nitriumRemoved < 0)
            return ReactionResult.NoReaction;

        mixture.AdjustMoles(Gas.Nitrium, -nitriumRemoved);
        mixture.AdjustMoles(Gas.Hydrogen, h2Produced);
        mixture.AdjustMoles(Gas.Nitrogen, nitrogenProduced);

        var energyReleased = efficiency * Atmospherics.NitriumDecompositionEnergy;
        var heatCap = atmosphereSystem.GetHeatCapacity(mixture, true);
        if (heatCap > Atmospherics.MinimumHeatCapacity)
            mixture.Temperature = Math.Min(Atmospherics.T0C + 98.8f, Math.Max((mixture.Temperature * heatCap + energyReleased) / heatCap, Atmospherics.TCMB));

        return ReactionResult.Reacting;
    }
}
