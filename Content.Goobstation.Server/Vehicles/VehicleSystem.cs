// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Vehicles;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using Content.Server.Body.Systems;
using Content.Shared.Atmos.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Body.Components;
using Robust.Shared.Containers;

namespace Content.Goobstation.Server.Vehicles;

public sealed class VehicleSystem : SharedVehicleSystem
{
    [Dependency] private readonly GasTankSystem _gasTank = default!;
    [Dependency] private readonly RespiratorSystem _respirator = default!;
    [Dependency] private readonly AtmosphereSystem _atmosphere = default!;
    [Dependency] private readonly InternalsSystem _internals = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<VehicleEnvironmentComponent, EntInsertedIntoContainerMessage>(OnEntInserted);
        SubscribeLocalEvent<VehicleEnvironmentComponent, EntRemovedFromContainerMessage>(OnEntRemoved);

        SubscribeLocalEvent<VehicleDriverComponent, InhaleLocationEvent>(OnInhale);
        SubscribeLocalEvent<VehicleDriverComponent, ExhaleLocationEvent>(OnExhale);
        SubscribeLocalEvent<VehicleDriverComponent, AtmosExposedGetAirEvent>(OnExpose);

        SubscribeLocalEvent<VehicleEnvironmentComponent, GetFilterAirEvent>(OnGetFilterAir);

    }

    private void OnEntInserted(EntityUid uid, VehicleEnvironmentComponent component, EntInsertedIntoContainerMessage args)
    {
        if (HasComp<GasTankComponent>(args.Entity))
        {
            component.GasTankEntity = args.Entity;
        }
    }

    private void OnEntRemoved(EntityUid uid, VehicleEnvironmentComponent component, EntRemovedFromContainerMessage args)
    {
        if (HasComp<GasTankComponent>(args.Entity))
        {
            component.GasTankEntity = args.Entity;
        }
    }

    private void OnInhale(EntityUid uid, VehicleDriverComponent component, InhaleLocationEvent args)
    {
        if (!TryComp<VehicleEnvironmentComponent>(component.Vehicle, out var vehicleEnvironment))
            return;

        // Check for if a driving entity is breathing through internals already.
        if (TryComp<InternalsComponent>(uid, out var internalsComp)
            && internalsComp is not null
            && _internals.AreInternalsWorking(internalsComp))
            return;

        if (vehicleEnvironment.RequireTank) // Checks if vehicle requires attached tank for environment, otherwise infinite air
        {
            if (vehicleEnvironment.GasTankEntity != null)
            {
                var gasTank = Comp<GasTankComponent>(vehicleEnvironment.GasTankEntity!.Value);
                args.Gas = _gasTank.RemoveAirVolume((vehicleEnvironment.GasTankEntity.Value, gasTank), args.Respirator.BreathVolume);
            }
        }
        else
            args.Gas = vehicleEnvironment.Air;
    }

    private void OnExhale(EntityUid uid, VehicleDriverComponent component, ExhaleLocationEvent args)
    {
        if (!TryComp<VehicleEnvironmentComponent>(component.Vehicle, out var vehicleEnvironment))
            return;

        // Check for if a driving entity is breathing through internals already.
        if (TryComp<InternalsComponent>(uid, out var internalsComp)
            && internalsComp is not null
            && _internals.AreInternalsWorking(internalsComp))
            return;

        if (vehicleEnvironment.RequireTank) // Checks if vehicle requires attached tank for environment, otherwise infinite air
        {
            if (vehicleEnvironment.GasTankEntity != null)
            {
                var gasTank = Comp<GasTankComponent>(vehicleEnvironment.GasTankEntity!.Value);
                args.Gas = gasTank.Air;
            }
        }
        else
            args.Gas = vehicleEnvironment.Air;
    }

    private void OnExpose(EntityUid uid, VehicleDriverComponent component, ref AtmosExposedGetAirEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp(component.Vehicle, out VehicleEnvironmentComponent? vehicleEnvironment)
            && vehicleEnvironment is null)
            return;

        if (vehicleEnvironment.RequireTank)
        {
            if (vehicleEnvironment.GasTankEntity != null)
            {
                var gasTank = Comp<GasTankComponent>(vehicleEnvironment.GasTankEntity!.Value);
                if (gasTank.Air.Pressure > gasTank.OutputPressure)
                {
                    // Triess to the exposed moles down to the output pressure of the tank.
                    // Note, P(new)/P(old) = n(new)/n(old) for an ideal gas at constant volume and temperature.
                    var copiedMixture = new GasMixture(gasTank.Air);
                    copiedMixture = copiedMixture.RemoveRatio(gasTank.OutputPressure / copiedMixture.Pressure);
                    args.Gas = copiedMixture;
                }
                else
                    args.Gas = gasTank.Air;

                args.Handled = true;
            }
        }
        else
        {
            args.Gas = vehicleEnvironment.Air;
            args.Handled = true;
        }
    }

    private void OnGetFilterAir(EntityUid uid, VehicleEnvironmentComponent component, ref GetFilterAirEvent args)
    {
        if (args.Air != null)
            return;

        // Check for if a driving entity is breathing through internals already.
        if (TryComp<InternalsComponent>(uid, out var internalsComp)
            && internalsComp is not null
            && _internals.AreInternalsWorking(internalsComp))
            return;

        if (component.RequireTank)
        {
            if (component.GasTankEntity != null)
            {
                var gasTank = Comp<GasTankComponent>(component.GasTankEntity!.Value);
                args.Air = gasTank.Air;
            }
        }
        else
            args.Air = component.Air;
        return;
    }
}
