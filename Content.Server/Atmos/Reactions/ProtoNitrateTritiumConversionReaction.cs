using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Reactions;
using JetBrains.Annotations;

namespace Content.Server.Atmos.Reactions;

[UsedImplicitly]
public sealed partial class ProtoNitrateTritiumConversionReaction : IGasReactionEffect
{
    public ReactionResult React(GasMixture mixture, IGasMixtureHolder? holder, AtmosphereSystem atmosphereSystem, float heatScale)
    {
        if (mixture.Temperature > 20f && mixture.GetMoles(Gas.HyperNoblium) >= 5f)
            return ReactionResult.NoReaction;

        var initPN = mixture.GetMoles(Gas.ProtoNitrate);
        var initTrit = mixture.GetMoles(Gas.Tritium);

        var temperature = mixture.Temperature;
        var producedAmount = Math.Min(temperature / 34f * initTrit * initPN / (initTrit + 10f * initPN), Math.Min(initTrit, initPN * 100f));

        if (initTrit - producedAmount < 0 || initPN - producedAmount * 0.01f < 0)
            return ReactionResult.NoReaction;

        mixture.AdjustMoles(Gas.ProtoNitrate, -producedAmount * 0.01f);
        mixture.AdjustMoles(Gas.Tritium, -producedAmount);
        mixture.AdjustMoles(Gas.Hydrogen, producedAmount);

        var energyReleased = producedAmount * Atmospherics.ProtoNitrateTritiumConversionEnergy;

        var heatCap = atmosphereSystem.GetHeatCapacity(mixture, true);
        if (heatCap > Atmospherics.MinimumHeatCapacity)
            mixture.Temperature = Math.Max((mixture.Temperature * heatCap + energyReleased) / heatCap, Atmospherics.TCMB);

        return ReactionResult.Reacting;
    }
}