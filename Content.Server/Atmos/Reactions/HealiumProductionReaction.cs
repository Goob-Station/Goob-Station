using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Reactions;
using JetBrains.Annotations;

namespace Content.Server.Atmos.Reactions;

/// <summary>
///     Assmos - /tg/ gases
///     Produces Healium by mixing BZ and Frezon at temperatures between 23K and 293K. Efficiency increases in colder temperatures.  
/// </summary>
[UsedImplicitly]
public sealed partial class HealiumProductionReaction : IGasReactionEffect
{
    public ReactionResult React(GasMixture mixture, IGasMixtureHolder? holder, AtmosphereSystem atmosphereSystem, float heatScale)
    {
        if (mixture.Temperature > 300f || mixture.Temperature < 22f)
            return ReactionResult.NoReaction;

        var initBZ = mixture.GetMoles(Gas.BZ);
        var initFrezon = mixture.GetMoles(Gas.Frezon);

        var efficiency = Math.Min(mixture.Temperature * 0.3f, Math.Min(initFrezon * 0.36f, initBZ * 4f));

        var bZRemoved = efficiency * 0.25f;
        var frezonRemoved = efficiency * 2.75f;
        var healiumProduced = efficiency * 3f;

        if (efficiency <= 0 || initFrezon - frezonRemoved < 0 || initBZ - bZRemoved < 0)
            return ReactionResult.NoReaction;

        mixture.AdjustMoles(Gas.BZ, -bZRemoved);
        mixture.AdjustMoles(Gas.Frezon, -frezonRemoved);
        mixture.AdjustMoles(Gas.Healium, healiumProduced);

        var energyReleased = efficiency * Atmospherics.HealiumProductionEnergy;
        var heatCap = atmosphereSystem.GetHeatCapacity(mixture, true);
        if (heatCap > Atmospherics.MinimumHeatCapacity)
            mixture.Temperature = Math.Max((mixture.Temperature * heatCap + energyReleased) / heatCap, Atmospherics.TCMB);

        return ReactionResult.Reacting;
    }
}
