using Content.Server.Atmos;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Reactions;
using JetBrains.Annotations;

namespace Content.Server._Funkystation.Atmos.Reactions;

/// <summary>
///     Assmos - /tg/ gases
///     The ignition of hydrogen in the presence of oxygen at temperatures above 373.15K.
/// </summary>
[UsedImplicitly]
[DataDefinition]
public sealed partial class HydrogenFireReaction : IGasReactionEffect
{
    public ReactionResult React(GasMixture mixture, IGasMixtureHolder? holder, AtmosphereSystem atmosphereSystem, float heatScale)
    {
        if (mixture.Temperature > 20f && mixture.GetMoles(Gas.HyperNoblium) >= 5f)
            return ReactionResult.NoReaction;

        var oldHeatCapacity = atmosphereSystem.GetHeatCapacity(mixture, true);
        var temperature = mixture.Temperature;
        var initHydrogen = mixture.GetMoles(Gas.Hydrogen);
        var initOxygen = mixture.GetMoles(Gas.Oxygen);
        var burnRateDelta = Atmospherics.HydrogenBurnRateDelta;
        var hydrogenOxygenFullBurn = Atmospherics.PlasmaOxygenFullburn;
        var burnedFuel = Math.Min(
            initHydrogen / burnRateDelta, Math.Min(
            initOxygen / (burnRateDelta * hydrogenOxygenFullBurn), Math.Min(
            initHydrogen,
            initOxygen * 2f))
        );

        mixture.ReactionResults[(byte) GasReaction.Fire] = 0f;

        if (burnedFuel <= 0 || initHydrogen - burnedFuel < 0 || initOxygen - (burnedFuel * 0.5f) < 0)
            return ReactionResult.NoReaction;

        mixture.AdjustMoles(Gas.Hydrogen, -burnedFuel);
        mixture.AdjustMoles(Gas.Oxygen, -(burnedFuel * 0.5f));
        mixture.AdjustMoles(Gas.WaterVapor, burnedFuel);

        var energyReleased = burnedFuel * Atmospherics.FireHydrogenEnergyReleased;
        var newHeatCapacity = atmosphereSystem.GetHeatCapacity(mixture, true);

        if (energyReleased > 0)
        {
            mixture.Temperature = Math.Max((mixture.Temperature * oldHeatCapacity + energyReleased) / newHeatCapacity, Atmospherics.TCMB);
        }

        if (holder is TileAtmosphere location)
        {
            temperature = mixture.Temperature;
            if (temperature > Atmospherics.FireMinimumTemperatureToExist)
            {
                atmosphereSystem.HotspotExpose(location, temperature, mixture.Volume);
            }
        }

        return mixture.ReactionResults[(byte) GasReaction.Fire] != 0 ? ReactionResult.Reacting : ReactionResult.NoReaction;
    }
}
