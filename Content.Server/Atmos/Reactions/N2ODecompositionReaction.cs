// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 Kevin Zheng <kevinz5000@gmail.com>
// SPDX-FileCopyrightText: 2023 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2024 Jezithyr <jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Reactions;
using JetBrains.Annotations;

namespace Content.Server.Atmos.Reactions;

/// <summary>
///     Decomposes Nitrous Oxide into Nitrogen and Oxygen.
/// </summary>
[UsedImplicitly]
public sealed partial class N2ODecompositionReaction : IGasReactionEffect
{
    public ReactionResult React(GasMixture mixture, IGasMixtureHolder? holder, AtmosphereSystem atmosphereSystem, float heatScale)
    {
        var cacheN2O = mixture.GetMoles(Gas.NitrousOxide);

        var burnedFuel = cacheN2O / Atmospherics.N2ODecompositionRate;

        if (burnedFuel <= 0 || cacheN2O - burnedFuel < 0)
            return ReactionResult.NoReaction;

        mixture.AdjustMoles(Gas.NitrousOxide, -burnedFuel);
        mixture.AdjustMoles(Gas.Nitrogen, burnedFuel);
        mixture.AdjustMoles(Gas.Oxygen, burnedFuel / 2);

        return ReactionResult.Reacting;
    }
}
