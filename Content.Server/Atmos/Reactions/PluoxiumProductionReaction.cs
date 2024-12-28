using Content.Server.Atmos.EntitySystems;
using Content.Server.EntityEffects.Effects;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Reactions;
using JetBrains.Annotations;

namespace Content.Server.Atmos.Reactions;

/// <summary>
///     Assmos - /tg/ gases
///     Consumes a tiny amount of tritium to convert CO2 and oxygen to pluoxium.
/// </summary>
[UsedImplicitly]
public sealed partial class PluoxiumProductionReaction : IGasReactionEffect
{
    public ReactionResult React(GasMixture mixture, IGasMixtureHolder? holder, AtmosphereSystem atmosphereSystem, float heatScale)
    {
        var initO2 = mixture.GetMoles(Gas.Oxygen);
        var initCO2 = mixture.GetMoles(Gas.CarbonDioxide);
        var initTrit = mixture.GetMoles(Gas.Tritium);

        float[] efficiencyList = {5f, initCO2, initO2 * 2f, initTrit * 100f};
        Array.Sort(efficiencyList);
        var producedAmount = efficiencyList[0];

        var co2Removed = producedAmount;
        var oxyRemoved = producedAmount * 0.5f;
        var tritRemoved = producedAmount * 0.01f;

        if (producedAmount <= 0 ||
            co2Removed > initCO2 ||
            oxyRemoved * 0.5 > initO2 ||
            tritRemoved * 0.01 > initTrit)
            return ReactionResult.NoReaction;

        var pluoxProduced = producedAmount;
        var hydroProduced = producedAmount * 0.01f;

        mixture.AdjustMoles(Gas.CarbonDioxide, -co2Removed);
        mixture.AdjustMoles(Gas.Oxygen, -oxyRemoved);
        mixture.AdjustMoles(Gas.Tritium, -tritRemoved);
        mixture.AdjustMoles(Gas.Pluoxium, pluoxProduced);
        mixture.AdjustMoles(Gas.WaterVapor, hydroProduced);

        var energyReleased = producedAmount * 250;
        var heatCap = atmosphereSystem.GetHeatCapacity(mixture, true);
        if (heatCap > Atmospherics.MinimumHeatCapacity)
            mixture.Temperature = Math.Max((mixture.Temperature * heatCap + energyReleased) / heatCap, Atmospherics.TCMB);

        return ReactionResult.Reacting;
    }
}
