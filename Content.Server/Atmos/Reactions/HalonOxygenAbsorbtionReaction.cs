using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Reactions;
using JetBrains.Annotations;

namespace Content.Server.Atmos.Reactions;

[UsedImplicitly]
public sealed partial class HalonOxygenAbsorptionReaction : IGasReactionEffect
{
    public ReactionResult React(GasMixture mixture, IGasMixtureHolder? holder, AtmosphereSystem atmosphereSystem, float heatScale)
    {
        if (mixture.Temperature > 20f && mixture.GetMoles(Gas.HyperNoblium) >= 5.0f)
            return ReactionResult.NoReaction;

        var initHalon = mixture.GetMoles(Gas.Halon);
        var initOxy = mixture.GetMoles(Gas.Oxygen);

        var temperature = mixture.Temperature;

        var heatEfficiency = Math.Min(temperature / (Atmospherics.FireMinimumTemperatureToExist * 10f), Math.Min(initHalon, initOxy*20f));
        if (heatEfficiency <= 0f || initHalon - heatEfficiency < 0f || initOxy - heatEfficiency * 20f < 0f)
            return ReactionResult.NoReaction;

        mixture.AdjustMoles(Gas.Halon, -heatEfficiency);
        mixture.AdjustMoles(Gas.Oxygen, -heatEfficiency*20f);
        mixture.AdjustMoles(Gas.CarbonDioxide, heatEfficiency*2.5f);

        var energyReleased = heatEfficiency * Atmospherics.HalonCombustionEnergy;
        var heatCap = atmosphereSystem.GetHeatCapacity(mixture, true);
        if (heatCap > Atmospherics.MinimumHeatCapacity)
            mixture.Temperature = Math.Max((mixture.Temperature * heatCap + energyReleased) / heatCap, Atmospherics.TCMB);

        return ReactionResult.Reacting;
    }
}