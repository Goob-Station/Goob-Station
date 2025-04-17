// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Jezithyr <jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Power.Generator;

namespace Content.Server.Power.Generator;

/// <seealso cref="GeneratorSystem"/>
/// <seealso cref="GeneratorExhaustGasComponent"/>
public sealed class GeneratorExhaustGasSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmosphere = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<GeneratorExhaustGasComponent, GeneratorUseFuel>(FuelUsed);
    }

    private void FuelUsed(EntityUid uid, GeneratorExhaustGasComponent component, GeneratorUseFuel args)
    {
        var exhaustMixture = new GasMixture();
        exhaustMixture.SetMoles(component.GasType, args.FuelUsed * component.MoleRatio);
        exhaustMixture.Temperature = component.Temperature;

        var environment = _atmosphere.GetContainingMixture(uid, false, true);
        if (environment != null)
            _atmosphere.Merge(environment, exhaustMixture);
    }
}