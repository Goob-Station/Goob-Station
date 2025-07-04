// SPDX-FileCopyrightText: 2024 12rabbits <53499656+12rabbits@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Alzore <140123969+Blackern5000@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 ArtisticRoomba <145879011+ArtisticRoomba@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Brandon Hu <103440971+Brandon-Huu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Dimastra <65184747+Dimastra@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Dimastra <dimastra@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Eoin Mcloughlin <helloworld@eoinrul.es>
// SPDX-FileCopyrightText: 2024 IProduceWidgets <107586145+IProduceWidgets@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 JIPDawg <51352440+JIPDawg@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 JIPDawg <JIPDawg93@gmail.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Menshin <Menshin@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Mervill <mervills.email@gmail.com>
// SPDX-FileCopyrightText: 2024 Moomoobeef <62638182+Moomoobeef@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 PJBot <pieterjan.briers+bot@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 PopGamer46 <yt1popgamer@gmail.com>
// SPDX-FileCopyrightText: 2024 PursuitInAshes <pursuitinashes@gmail.com>
// SPDX-FileCopyrightText: 2024 QueerNB <176353696+QueerNB@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Saphire Lattice <lattice@saphi.re>
// SPDX-FileCopyrightText: 2024 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Simon <63975668+Simyon264@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Spessmann <156740760+Spessmann@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Thomas <87614336+Aeshus@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Winkarst <74284083+Winkarst-cpu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 eoineoineoin <github@eoinrul.es>
// SPDX-FileCopyrightText: 2024 github-actions[bot] <41898282+github-actions[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 stellar-novas <stellar_novas@riseup.net>
// SPDX-FileCopyrightText: 2024 themias <89101928+themias@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Piping.Components;
using Content.Server.Atmos.Piping.Unary.Components;
using Content.Server.Popups;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.Atmos.Piping.Portable.Components;
using Content.Shared.Atmos.Visuals;
using Content.Shared.Power;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;

namespace Content.Server.Atmos.Portable;

public sealed class SpaceHeaterSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly PowerReceiverSystem _power = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly UserInterfaceSystem _userInterfaceSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpaceHeaterComponent, ActivatableUIOpenAttemptEvent>(OnUIActivationAttempt);
        SubscribeLocalEvent<SpaceHeaterComponent, BeforeActivatableUIOpenEvent>(OnBeforeOpened);

        SubscribeLocalEvent<SpaceHeaterComponent, AtmosDeviceUpdateEvent>(OnDeviceUpdated);
        SubscribeLocalEvent<SpaceHeaterComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<SpaceHeaterComponent, PowerChangedEvent>(OnPowerChanged);

        SubscribeLocalEvent<SpaceHeaterComponent, SpaceHeaterChangeModeMessage>(OnModeChanged);
        SubscribeLocalEvent<SpaceHeaterComponent, SpaceHeaterChangePowerLevelMessage>(OnPowerLevelChanged);
        SubscribeLocalEvent<SpaceHeaterComponent, SpaceHeaterChangeTemperatureMessage>(OnTemperatureChanged);
        SubscribeLocalEvent<SpaceHeaterComponent, SpaceHeaterToggleMessage>(OnToggle);
    }

    private void OnInit(EntityUid uid, SpaceHeaterComponent spaceHeater, MapInitEvent args)
    {
        if (!TryComp<GasThermoMachineComponent>(uid, out var thermoMachine))
            return;
        thermoMachine.Cp = spaceHeater.HeatingCp;
        thermoMachine.HeatCapacity = spaceHeater.PowerConsumption;
    }

    private void OnBeforeOpened(EntityUid uid, SpaceHeaterComponent spaceHeater, BeforeActivatableUIOpenEvent args)
    {
        DirtyUI(uid, spaceHeater);
    }

    private void OnUIActivationAttempt(EntityUid uid, SpaceHeaterComponent spaceHeater, ActivatableUIOpenAttemptEvent args)
    {
        if (!Comp<TransformComponent>(uid).Anchored)
        {
            _popup.PopupEntity(Loc.GetString("comp-space-heater-unanchored", ("device", Loc.GetString("comp-space-heater-device-name"))), uid, args.User);
            args.Cancel();
        }
    }

    private void OnDeviceUpdated(EntityUid uid, SpaceHeaterComponent spaceHeater, ref AtmosDeviceUpdateEvent args)
    {
        if (!_power.IsPowered(uid)
            || !TryComp<GasThermoMachineComponent>(uid, out var thermoMachine))
        {
            return;
        }

        UpdateAppearance(uid);

        // If in automatic temperature mode, check if we need to adjust the heat exchange direction
        if (spaceHeater.Mode == SpaceHeaterMode.Auto)
        {
            var environment = _atmosphereSystem.GetContainingMixture(uid, args.Grid, args.Map);
            if (environment == null)
                return;

            if (environment.Temperature <= thermoMachine.TargetTemperature - (thermoMachine.TemperatureTolerance + spaceHeater.AutoModeSwitchThreshold))
            {
                thermoMachine.Cp = spaceHeater.HeatingCp;
            }
            else if (environment.Temperature >= thermoMachine.TargetTemperature + (thermoMachine.TemperatureTolerance + spaceHeater.AutoModeSwitchThreshold))
            {
                thermoMachine.Cp = spaceHeater.CoolingCp;
            }
        }
    }

    private void OnPowerChanged(EntityUid uid, SpaceHeaterComponent spaceHeater, ref PowerChangedEvent args)
    {
        UpdateAppearance(uid);
        DirtyUI(uid, spaceHeater);
    }

    private void OnToggle(EntityUid uid, SpaceHeaterComponent spaceHeater, SpaceHeaterToggleMessage args)
    {
        ApcPowerReceiverComponent? powerReceiver = null;
        if (!Resolve(uid, ref powerReceiver))
            return;

        _power.TogglePower(uid);

        UpdateAppearance(uid);
        DirtyUI(uid, spaceHeater);
    }

    private void OnTemperatureChanged(EntityUid uid, SpaceHeaterComponent spaceHeater, SpaceHeaterChangeTemperatureMessage args)
    {
        if (!TryComp<GasThermoMachineComponent>(uid, out var thermoMachine))
            return;

        thermoMachine.TargetTemperature = float.Clamp(thermoMachine.TargetTemperature + args.Temperature, thermoMachine.MinTemperature, thermoMachine.MaxTemperature);

        UpdateAppearance(uid);
        DirtyUI(uid, spaceHeater);
    }

    private void OnModeChanged(EntityUid uid, SpaceHeaterComponent spaceHeater, SpaceHeaterChangeModeMessage args)
    {
        if (!TryComp<GasThermoMachineComponent>(uid, out var thermoMachine))
            return;

        spaceHeater.Mode = args.Mode;

        if (spaceHeater.Mode == SpaceHeaterMode.Heat)
            thermoMachine.Cp = spaceHeater.HeatingCp;
        else if (spaceHeater.Mode == SpaceHeaterMode.Cool)
            thermoMachine.Cp = spaceHeater.CoolingCp;

        DirtyUI(uid, spaceHeater);
    }

    private void OnPowerLevelChanged(EntityUid uid, SpaceHeaterComponent spaceHeater, SpaceHeaterChangePowerLevelMessage args)
    {
        if (!TryComp<GasThermoMachineComponent>(uid, out var thermoMachine))
            return;

        spaceHeater.PowerLevel = args.PowerLevel;

        switch (spaceHeater.PowerLevel)
        {
            case SpaceHeaterPowerLevel.Low:
                thermoMachine.HeatCapacity = spaceHeater.PowerConsumption / 2;
                break;

            case SpaceHeaterPowerLevel.Medium:
                thermoMachine.HeatCapacity = spaceHeater.PowerConsumption;
                break;

            case SpaceHeaterPowerLevel.High:
                thermoMachine.HeatCapacity = spaceHeater.PowerConsumption * 2;
                break;
        }

        DirtyUI(uid, spaceHeater);
    }

    private void DirtyUI(EntityUid uid, SpaceHeaterComponent? spaceHeater)
    {
        if (!Resolve(uid, ref spaceHeater)
            || !TryComp<GasThermoMachineComponent>(uid, out var thermoMachine)
            || !TryComp<ApcPowerReceiverComponent>(uid, out var powerReceiver))
        {
            return;
        }
        _userInterfaceSystem.SetUiState(uid, SpaceHeaterUiKey.Key,
            new SpaceHeaterBoundUserInterfaceState(spaceHeater.MinTemperature, spaceHeater.MaxTemperature, thermoMachine.TargetTemperature, !powerReceiver.PowerDisabled, spaceHeater.Mode, spaceHeater.PowerLevel));
    }

    private void UpdateAppearance(EntityUid uid)
    {
        if (!_power.IsPowered(uid) || !TryComp<GasThermoMachineComponent>(uid, out var thermoMachine))
        {
            _appearance.SetData(uid, SpaceHeaterVisuals.State, SpaceHeaterState.Off);
            return;
        }

        if (thermoMachine.LastEnergyDelta > 0)
        {
            _appearance.SetData(uid, SpaceHeaterVisuals.State, SpaceHeaterState.Heating);
        }
        else if (thermoMachine.LastEnergyDelta < 0)
        {
            _appearance.SetData(uid, SpaceHeaterVisuals.State, SpaceHeaterState.Cooling);
        }
        else
        {
            _appearance.SetData(uid, SpaceHeaterVisuals.State, SpaceHeaterState.StandBy);
        }
    }
}