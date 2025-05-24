using Content.Server.Atmos;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Reactions;
using JetBrains.Annotations;

namespace Content.Goobstation.Server.Atmos.Reactions;

/// <summary>
///     Assmos - /tg/ gases
///     Consumes a tiny amount of tritium to convert CO2 and oxygen to pluoxium.
/// </summary>
[UsedImplicitly]
public sealed partial class PluoxiumTritiumProductionReaction : IGasReactionEffect
{
    public ReactionResult React(GasMixture mixture, IGasMixtureHolder? holder, AtmosphereSystem atmosphereSystem, float heatScale)
    {
        if (mixture.Temperature > 20f && mixture.GetMoles(Gas.HyperNoblium) >= 5f)
            return ReactionResult.NoReaction;

        var initO2 = mixture.GetMoles(Gas.Oxygen);
        var initCO2 = mixture.GetMoles(Gas.CarbonDioxide);
        var initTrit = mixture.GetMoles(Gas.Tritium);

        float[] efficiencies = [5f, initCO2, initO2 * 2f, initTrit * 100f];
        Array.Sort(efficiencies);
        var producedAmount = efficiencies[0];

        if (producedAmount <= 0)
            return ReactionResult.NoReaction;

        var co2Removed = producedAmount;
        var oxyRemoved = producedAmount * 0.5f;
        var tritRemoved = producedAmount * 0.01f;
        var pluoxProduced = producedAmount;
        var hydroProduced = producedAmount * 0.01f;

        mixture.AdjustMoles(Gas.CarbonDioxide, -co2Removed);
        mixture.AdjustMoles(Gas.Oxygen, -oxyRemoved);
        mixture.AdjustMoles(Gas.Tritium, -tritRemoved);
        mixture.AdjustMoles(Gas.Pluoxium, pluoxProduced);
        mixture.AdjustMoles(Gas.Hydrogen, hydroProduced);

        var energyReleased = producedAmount * Atmospherics.PluoxiumProductionEnergy;
        var heatCap = atmosphereSystem.GetHeatCapacity(mixture, true);
        if (heatCap > Atmospherics.MinimumHeatCapacity)
            mixture.Temperature = Math.Max((mixture.Temperature * heatCap + energyReleased) / heatCap, Atmospherics.TCMB);

        return ReactionResult.Reacting;
    }
}
