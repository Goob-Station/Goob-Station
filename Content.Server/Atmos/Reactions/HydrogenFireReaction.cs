// Assmos - /tg/ gases
// Essentially the exact same as a tritium fire but without the still TODO radioactivity of a tritium burn. 

using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Reactions;
using JetBrains.Annotations;

namespace Content.Server.Atmos.Reactions
{
    [UsedImplicitly]
    [DataDefinition]
    public sealed partial class HydrogenFireReaction : IGasReactionEffect
    {
        public ReactionResult React(GasMixture mixture, IGasMixtureHolder? holder, AtmosphereSystem atmosphereSystem, float heatScale)
        {
            if (mixture.Temperature > 20f && mixture.GetMoles(Gas.HyperNoblium) >= 5f)
                return ReactionResult.NoReaction;

            var energyReleased = 0f;
            var oldHeatCapacity = atmosphereSystem.GetHeatCapacity(mixture, true);
            var temperature = mixture.Temperature;
            var location = holder as TileAtmosphere;
            mixture.ReactionResults[(byte)GasReaction.Fire] = 0f;
            var burnedFuel = 0f;
            var initialH2 = mixture.GetMoles(Gas.Hydrogen);

            if (mixture.GetMoles(Gas.Oxygen) < initialH2 ||
                Atmospherics.MinimumHydrogenOxyburnEnergy > (temperature * oldHeatCapacity * heatScale))
            {
                burnedFuel = mixture.GetMoles(Gas.Oxygen) / Atmospherics.HydrogenBurnOxyFactor;
                if (burnedFuel > initialH2)
                    burnedFuel = initialH2;

                mixture.AdjustMoles(Gas.Hydrogen, -burnedFuel);
            }
            else
            {
                burnedFuel = initialH2;
                mixture.SetMoles(Gas.Hydrogen, mixture.GetMoles(Gas.Hydrogen) * (1 - 1 / Atmospherics.HydrogenBurnH2Factor));
                mixture.AdjustMoles(Gas.Oxygen, -mixture.GetMoles(Gas.Hydrogen));
                energyReleased += (Atmospherics.FireHydrogenEnergyReleased * burnedFuel * (Atmospherics.HydrogenBurnH2Factor - 1));
            }

            if (burnedFuel > 0)
            {
                energyReleased += (Atmospherics.FireHydrogenEnergyReleased * burnedFuel);

                mixture.AdjustMoles(Gas.WaterVapor, burnedFuel);

                mixture.ReactionResults[(byte)GasReaction.Fire] += burnedFuel;
            }

            energyReleased /= heatScale; // adjust energy to make sure speedup doesn't cause mega temperature rise
            if (energyReleased > 0)
            {
                var newHeatCapacity = atmosphereSystem.GetHeatCapacity(mixture, true);
                if (newHeatCapacity > Atmospherics.MinimumHeatCapacity)
                    mixture.Temperature = ((temperature * oldHeatCapacity + energyReleased) / newHeatCapacity);
            }

            if (location != null)
            {
                temperature = mixture.Temperature;
                if (temperature > Atmospherics.FireMinimumTemperatureToExist)
                {
                    atmosphereSystem.HotspotExpose(location, temperature, mixture.Volume);
                }
            }

            return mixture.ReactionResults[(byte)GasReaction.Fire] != 0 ? ReactionResult.Reacting : ReactionResult.NoReaction;
        }
    }
}
