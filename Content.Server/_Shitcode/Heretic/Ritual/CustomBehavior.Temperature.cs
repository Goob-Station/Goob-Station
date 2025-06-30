// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Heretic.Prototypes;

namespace Content.Server.Heretic.Ritual;

[Virtual] public sealed partial class RitualTemperatureBehavior : RitualCustomBehavior
{
    /// <summary>
    ///     Min temp in celsius
    /// </summary>
    [DataField] public float MinThreshold = 0f;

    /// <summary>
    ///     Max temp in celsius
    /// </summary>
    [DataField] public float MaxThreshold = float.PositiveInfinity;

    private AtmosphereSystem _atmos = default!;

    public override bool Execute(RitualData args, out string? outstr)
    {
        outstr = null;

        _atmos = args.EntityManager.System<AtmosphereSystem>();

        var mix = _atmos.GetTileMixture(args.Platform);

        if (mix == null || mix.TotalMoles == 0) // just accept space as it is
            return true;

        if (mix.Temperature > Atmospherics.T0C + MaxThreshold)
        {
            outstr = Loc.GetString("heretic-ritual-fail-temperature-hot");
            return false;
        }
        if (mix.Temperature > Atmospherics.T0C + MinThreshold)
        {
            outstr = Loc.GetString("heretic-ritual-fail-temperature-cold");
            return false;
        }

        return true;
    }

    public override void Finalize(RitualData args)
    {
        // do nothing
    }
}