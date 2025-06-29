using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Reactions;
using JetBrains.Annotations;

namespace Content.Server.Atmos.Reactions;

[UsedImplicitly]
public sealed partial class ProtoNitrateProductionReaction : IGasReactionEffect
{
    public ReactionResult React(GasMixture mixture, IGasMixtureHolder? holder, AtmosphereSystem atmosphereSystem, float heatScale)
    {
        if (mixture.Temperature > 20f && mixture.GetMoles(Gas.HyperNoblium) >= 5f)
            return ReactionResult.NoReaction;

        var initPluox = mixture.GetMoles(Gas.Pluoxium);
        var initH2 = mixture.GetMoles(Gas.Hydrogen);

        var temperature = mixture.Temperature;
        var heatEfficiency = Math.Min(temperature * 0.005f, Math.Min(initPluox * 5f, initH2 * 0.5f));

        if (heatEfficiency <= 0 || initPluox - heatEfficiency * 0.2f < 0 || initH2 - heatEfficiency * 2f < 0)
            return ReactionResult.NoReaction;

        mixture.AdjustMoles(Gas.Hydrogen, -heatEfficiency * 2f);
        mixture.AdjustMoles(Gas.Pluoxium, -heatEfficiency * 0.2f);
        mixture.AdjustMoles(Gas.ProtoNitrate, heatEfficiency * 0.2f);

        var energyReleased = heatEfficiency * Atmospherics.ProtoNitrateProductionEnergy;

        var heatCap = atmosphereSystem.GetHeatCapacity(mixture, true);
        if (heatCap > Atmospherics.MinimumHeatCapacity)
            mixture.Temperature = Math.Max((mixture.Temperature * heatCap + energyReleased) / heatCap, Atmospherics.TCMB);

        return ReactionResult.Reacting;
    }
}