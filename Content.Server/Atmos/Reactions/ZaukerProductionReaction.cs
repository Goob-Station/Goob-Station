using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Reactions;
using JetBrains.Annotations;

namespace Content.Server.Atmos.Reactions;

[UsedImplicitly]
public sealed partial class ZaukerProductionReaction : IGasReactionEffect
{
    public ReactionResult React(GasMixture mixture, IGasMixtureHolder? holder, AtmosphereSystem atmosphereSystem, float heatScale)
    {
        if (mixture.Temperature > 20f && mixture.GetMoles(Gas.HyperNoblium) >= 5f)
            return ReactionResult.NoReaction;

        var initHyperNob = mixture.GetMoles(Gas.HyperNoblium);
        var initNitrium = mixture.GetMoles(Gas.Nitrium);

        var temperature = mixture.Temperature;
        var heatEfficiency = Math.Min(temperature * Atmospherics.ZaukerTemperatureScale, Math.Min(initHyperNob * 100f, initNitrium * 20f));

        if (heatEfficiency <= 0 || initHyperNob - heatEfficiency * 0.01f < 0 || initNitrium - heatEfficiency * 0.5f < 0)
            return ReactionResult.NoReaction;

        mixture.AdjustMoles(Gas.HyperNoblium, -heatEfficiency * 0.01f);
        mixture.AdjustMoles(Gas.Nitrium, -heatEfficiency * 0.5f);
        mixture.AdjustMoles(Gas.Zauker, heatEfficiency * 0.5f);

        var energyReleased = heatEfficiency * Atmospherics.ZaukerProductionEnergy;

        var heatCap = atmosphereSystem.GetHeatCapacity(mixture, true);
        if (heatCap > Atmospherics.MinimumHeatCapacity)
            mixture.Temperature = Math.Max((mixture.Temperature * heatCap - energyReleased) / heatCap, Atmospherics.TCMB);

        return ReactionResult.Reacting;
    }
}