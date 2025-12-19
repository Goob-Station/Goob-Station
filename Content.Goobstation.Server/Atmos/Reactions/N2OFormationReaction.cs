// SPDX-FileCopyrightText: 2025 lambdatiger <11843718+lambdatiger@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-lat

using Content.Goobstation.Shared.Atmos;
using Content.Server.Atmos;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Reactions;
using JetBrains.Annotations;

namespace Content.Goobstation.Server.Atmos.Reactions;

/// <summary>
///     From /tg/ gases
///     Forms of N2O from a 2:1 Nitrogen and Oxygen mix catalyzed with BZ. Exothermic reaction.
/// </summary>
[UsedImplicitly]
public sealed partial class N2OFormationReaction : IGasReactionEffect
{
    public ReactionResult React(GasMixture mixture, IGasMixtureHolder? holder, AtmosphereSystem atmosphereSystem, float heatScale)
    {
        var initBZ = mixture.GetMoles(Gas.BZ);
        if (initBZ < 5 || mixture.Temperature < 200f || mixture.Temperature > 250f)
            return ReactionResult.NoReaction;

        var initOxygen = mixture.GetMoles(Gas.Oxygen);
        var initNitrogen = mixture.GetMoles(Gas.Nitrogen);

        var n2oAdded = Math.Min(initOxygen * 0.5f, initNitrogen); // collect reaction amount, could be more but it's more interesting to limit reaction rate
        if (initNitrogen < n2oAdded || initOxygen * 0.5f < n2oAdded)
            return ReactionResult.NoReaction;

        mixture.AdjustMoles(Gas.NitrousOxide, n2oAdded);
        mixture.AdjustMoles(Gas.Nitrogen, -n2oAdded);
        mixture.AdjustMoles(Gas.Oxygen, -0.5f * n2oAdded);

        var heatCap = atmosphereSystem.GetHeatCapacity(mixture, true);
        if (heatCap > Atmospherics.MinimumHeatCapacity)
            mixture.Temperature = Math.Max(mixture.Temperature + (n2oAdded * GoobAtmospherics.N2OFormationEnergy) / heatCap, Atmospherics.TCMB);

        return ReactionResult.Reacting;
    }
}