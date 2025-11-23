using Content.Goobstation.Common.Materials._FarHorizons.Systems;
using Content.Goobstation.Shared.Power._FarHorizons.Power.Generation.FissionGenerator;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Piping.Components;
using Content.Shared.Atmos;
using Robust.Shared.Random;
using DependencyAttribute = Robust.Shared.IoC.DependencyAttribute;

namespace Content.Goobstation.Server.Power._FarHorizons.Power.Generation.FissionGenerator;

// Ported and modified from goonstation by Jhrushbe.
// CC-BY-NC-SA-3.0
// https://github.com/goonstation/goonstation/blob/ff86b044/code/obj/nuclearreactor/reactorcomponents.dm

public sealed class ReactorPartSystem : SharedReactorPartSystem
{
    [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    /// <summary>
    /// Processes gas flowing through a reactor part.
    /// </summary>
    /// <param name="reactorPart">The reactor part.</param>
    /// <param name="reactorEnt">The entity representing the reactor this part is inserted into.</param>
    /// <param name="inGas">The gas to be processed.</param>
    /// <returns></returns>
    public GasMixture? ProcessGas(Shared.Power._FarHorizons.Power.Generation.FissionGenerator.ReactorPartComponent reactorPart, Entity<Shared.Power._FarHorizons.Power.Generation.FissionGenerator.NuclearReactorComponent> reactorEnt, AtmosDeviceUpdateEvent args, GasMixture inGas)
    {
        if (reactorPart.RodType != (byte)Shared.Power._FarHorizons.Power.Generation.FissionGenerator.ReactorPartComponent.RodTypes.GasChannel)
            return null;

        GasMixture? ProcessedGas = null;
        if (reactorPart.AirContents != null)
        {
            var compTemp = reactorPart.Temperature;
            var gasTemp = reactorPart.AirContents.Temperature;

            var DeltaT = compTemp - gasTemp;
            var DeltaTr = (compTemp + gasTemp) * (compTemp - gasTemp) * (Math.Pow(compTemp, 2) + Math.Pow(gasTemp, 2));

            var k = MaterialSystem.CalculateHeatTransferCoefficient(reactorPart.Properties, null);
            var A = reactorPart.GasThermalCrossSection * (0.4 * 8);

            var ThermalEnergy = _atmosphereSystem.GetThermalEnergy(reactorPart.AirContents);

            var Hottest = Math.Max(gasTemp, compTemp);
            var Coldest = Math.Min(gasTemp, compTemp);

            var MaxDeltaE = Math.Clamp((k * A * DeltaT) + (5.67037442e-8 * A * DeltaTr),
                (compTemp * reactorPart.ThermalMass) - (Hottest * reactorPart.ThermalMass),
                (compTemp * reactorPart.ThermalMass) - (Coldest * reactorPart.ThermalMass));

            reactorPart.AirContents.Temperature = (float)Math.Clamp(gasTemp +
                (MaxDeltaE / _atmosphereSystem.GetHeatCapacity(reactorPart.AirContents, true)), Coldest, Hottest);

            reactorPart.Temperature = (float)Math.Clamp(compTemp -
                ((_atmosphereSystem.GetThermalEnergy(reactorPart.AirContents) - ThermalEnergy) / reactorPart.ThermalMass), Coldest, Hottest);

            if (gasTemp < 0 || compTemp < 0)
                throw new Exception("Reactor part temperature went below 0k.");

            if (reactorPart.Melted)
            {
                var T = _atmosphereSystem.GetTileMixture(reactorEnt.Owner, excite: true);
                if (T != null)
                    _atmosphereSystem.Merge(T, reactorPart.AirContents);
            }
            else
                ProcessedGas = reactorPart.AirContents;
        }

        if (inGas != null && _atmosphereSystem.GetThermalEnergy(inGas) > 0)
        {
            reactorPart.AirContents = inGas.RemoveVolume(Math.Min(reactorPart.GasVolume * _atmosphereSystem.PumpSpeedup() * args.dt, inGas.Volume));
            reactorPart.AirContents.Volume = reactorPart.GasVolume;

            if (reactorPart.AirContents != null && reactorPart.AirContents.TotalMoles < 1)
            {
                if (ProcessedGas != null)
                {
                    _atmosphereSystem.Merge(ProcessedGas, reactorPart.AirContents);
                    reactorPart.AirContents.Clear();
                }
                else
                {
                    ProcessedGas = reactorPart.AirContents;
                    reactorPart.AirContents.Clear();
                }
            }
        }
        return ProcessedGas;
    }

    /// <inheritdoc/>
    public override List<ReactorNeutron> ProcessNeutronsGas(Shared.Power._FarHorizons.Power.Generation.FissionGenerator.ReactorPartComponent reactorPart, List<ReactorNeutron> neutrons)
    {
        if (reactorPart.AirContents == null) return neutrons;

        var flux = new List<ReactorNeutron>(neutrons);
        foreach (var neutron in flux)
        {
            if (neutron.velocity > 0)
            {
                var neutronCount = GasNeutronInteract(reactorPart);
                if (neutronCount > 1)
                    for (var i = 0; i < neutronCount; i++)
                        neutrons.Add(new() { dir = _random.NextAngle().GetDir(), velocity = _random.Next(1, 3 + 1) });
                else
                    neutrons.Remove(neutron);
            }
        }

        return neutrons;
    }

    /// <summary>
    /// Determines the number of additional neutrons the gas makes.
    /// </summary>
    /// <param name="reactorPart"></param>
    /// <returns></returns>
    private int GasNeutronInteract(Shared.Power._FarHorizons.Power.Generation.FissionGenerator.ReactorPartComponent reactorPart)
    {
        if (reactorPart.AirContents == null)
            return 0;

        var neutronCount = 0;
        var gas = reactorPart.AirContents;

        if (gas.GetMoles(Gas.Plasma) > 1)
        {
            var reactMolPerLiter = 0.25;

            var plasma = gas.GetMoles(Gas.Plasma);
            var plasmaReactCount = (int)Math.Round((plasma - (plasma % (reactMolPerLiter * gas.Volume))) / (reactMolPerLiter * gas.Volume))
                + (Prob(plasma - (plasma % (reactMolPerLiter * gas.Volume))) ? 1 : 0);
            plasmaReactCount = _random.Next(0, plasmaReactCount + 1);
            gas.AdjustMoles(Gas.Plasma, plasmaReactCount * -0.5f);
            gas.AdjustMoles(Gas.Tritium, plasmaReactCount * 2);
            neutronCount += plasmaReactCount;
        }

        if (gas.GetMoles(Gas.CarbonDioxide) > 1)
        {
            var reactMolPerLiter = 0.4;

            var co2 = gas.GetMoles(Gas.CarbonDioxide);
            var co2ReactCount = (int)Math.Round((co2 - (co2 % (reactMolPerLiter * gas.Volume))) / (reactMolPerLiter * gas.Volume))
                + (Prob(co2 - (co2 % (reactMolPerLiter * gas.Volume))) ? 1 : 0);
            co2ReactCount = _random.Next(0, co2ReactCount + 1);
            reactorPart.Temperature += Math.Min(co2ReactCount, neutronCount);
            neutronCount -= Math.Min(co2ReactCount, neutronCount);
        }

        if (gas.GetMoles(Gas.Tritium) > 1)
        {
            var reactMolPerLiter = 0.5;

            var tritium = gas.GetMoles(Gas.Tritium);
            var tritiumReactCount = (int)Math.Round((tritium - (tritium % (reactMolPerLiter * gas.Volume))) / (reactMolPerLiter * gas.Volume))
                + (Prob(tritium - (tritium % (reactMolPerLiter * gas.Volume))) ? 1 : 0);
            if (tritiumReactCount > 0)
            {
                gas.AdjustMoles(Gas.Tritium, -1 * tritiumReactCount);
                reactorPart.Temperature += 1;
                switch (_random.Next(0, 5))
                {
                    case 0:
                        gas.AdjustMoles(Gas.Oxygen, 0.5f * tritiumReactCount);
                        break;
                    case 1:
                        gas.AdjustMoles(Gas.Nitrogen, 0.5f * tritiumReactCount);
                        break;
                    case 2:
                        gas.AdjustMoles(Gas.Ammonia, 0.1f * tritiumReactCount);
                        break;
                    case 3:
                        gas.AdjustMoles(Gas.NitrousOxide, 0.1f * tritiumReactCount);
                        break;
                    case 4:
                        gas.AdjustMoles(Gas.Frezon, 0.1f * tritiumReactCount);
                        break;
                    default:
                        break;
                }
            }
        }

        return neutronCount;
    }
}
