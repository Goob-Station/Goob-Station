using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Reactions;
using JetBrains.Annotations;

namespace Content.Server.Atmos.Reactions;

/// <summary>
///     Assmos - /tg/ gases
///     Forms BZ from mixing Plasma and Nitrous Oxide at low pressure. Also decomposes Nitrous Oxide when there are more than 3 parts Plasma per N2O.
/// </summary>
[UsedImplicitly]
public sealed partial class BZFormationReaction : IGasReactionEffect
{
    public ReactionResult React(GasMixture mixture, IGasMixtureHolder? holder, AtmosphereSystem atmosphereSystem, float heatScale)
    {
        var initN2O = mixture.GetMoles(Gas.NitrousOxide);
        var initPlasma = mixture.GetMoles(Gas.Plasma);
        var pressure = mixture.Pressure;
        var volume = mixture.Volume;

        var environmentEfficiency = volume / pressure; // more volume and less pressure gives better rates
        var ratioEfficiency = Math.Min(initN2O / initPlasma, 1f); // less n2o than plasma gives lower rates
        var bzFormed = Math.Min(0.01f * ratioEfficiency * environmentEfficiency, Math.Min(initN2O * 2.5f, initPlasma * 1.25f));

        var nitrousOxideDecomposed =  Math.Max(4f * (initPlasma / (initN2O + initPlasma) - 0.75f), 0);
        var nitrogenAdded = 0f;
        var oxygenAdded = 0f;
        if (nitrousOxideDecomposed > 0) 
        {
            var amountDecomposed = 0.4f * bzFormed * nitrousOxideDecomposed;
            nitrogenAdded = amountDecomposed;
            oxygenAdded = 0.5f * amountDecomposed;
        }
        var bzAdded = bzFormed * (1f-nitrousOxideDecomposed);
        var n2oRemoved = 0.4f * bzFormed;
        var plasmaRemoved = 0.8f * bzFormed * (1f-nitrousOxideDecomposed);

        if (n2oRemoved > initN2O || plasmaRemoved > initPlasma)
            return ReactionResult.NoReaction;

        mixture.AdjustMoles(Gas.NitrousOxide, -n2oRemoved);
        mixture.AdjustMoles(Gas.Plasma, -plasmaRemoved);
        mixture.AdjustMoles(Gas.Nitrogen, nitrogenAdded);
        mixture.AdjustMoles(Gas.Oxygen, oxygenAdded);
        mixture.AdjustMoles(Gas.BZ, bzAdded);

        var energyReleased = bzFormed * (Atmospherics.BZFormationEnergy + nitrousOxideDecomposed);
        var heatCap = atmosphereSystem.GetHeatCapacity(mixture, true);
        if (heatCap > Atmospherics.MinimumHeatCapacity)
            mixture.Temperature = Math.Max((mixture.Temperature * heatCap + energyReleased) / heatCap, Atmospherics.TCMB);

        return ReactionResult.Reacting;
    }
}