// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

// Assmos - /tg/ gases
using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Piping.Components;
using Content.Server.Popups;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.Atmos;
using Content.Goobstation.Shared.Atmos.Visuals;
using Content.Shared.Interaction;
using Content.Shared.Power;

namespace Content.Goobstation.Server.Atmos.Portable;

public sealed class ElectrolyzerSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly PowerReceiverSystem _power = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly GasTileOverlaySystem _gasOverlaySystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ElectrolyzerComponent, AtmosDeviceUpdateEvent>(OnDeviceUpdated);
        SubscribeLocalEvent<ElectrolyzerComponent, PowerChangedEvent>(OnPowerChanged);
        SubscribeLocalEvent<ElectrolyzerComponent, ActivateInWorldEvent>(OnActivate);
    }

    private void OnPowerChanged(EntityUid uid, ElectrolyzerComponent electrolyzer, ref PowerChangedEvent args)
    {
        UpdateAppearance(uid);
    }

    private void OnActivate(EntityUid uid, ElectrolyzerComponent electrolyzer, ActivateInWorldEvent args)
    {
        if (args.Handled)
            return;

        ApcPowerReceiverComponent? powerReceiver = null;
        if (!Resolve(uid, ref powerReceiver))
            return;

        _power.TogglePower(uid);

        UpdateAppearance(uid);
    }

    private void UpdateAppearance(EntityUid uid)
    {
        if (!_power.IsPowered(uid))
        {
            _appearance.SetData(uid, ElectrolyzerVisuals.State, ElectrolyzerState.Off);
            return;
        }
        else
        {
            _appearance.SetData(uid, ElectrolyzerVisuals.State, ElectrolyzerState.On);
        }
    }

    private void OnDeviceUpdated(EntityUid uid, ElectrolyzerComponent electrolyzer, ref AtmosDeviceUpdateEvent args)
    {
        if (!(_power.IsPowered(uid) && TryComp<ApcPowerReceiverComponent>(uid, out var receiver)))
            return;

        UpdateAppearance(uid);

        var mixture = _atmosphereSystem.GetContainingMixture(uid, args.Grid, args.Map);
        if (mixture is null) return;

        var initH2O = mixture.GetMoles(Gas.WaterVapor);
        var initHyperNob = mixture.GetMoles(Gas.HyperNoblium);
        var initBZ = mixture.GetMoles(Gas.BZ);
        var temperature = mixture.Temperature;
        const float workingPower = 1.8f;
        const float powerEfficiency = 1f;
        float powerLoad = 100f;
        float activeLoad = (4200f * (3f * workingPower) * workingPower) / (powerEfficiency + workingPower);

        if (initH2O > 0.05f)
        {
            var maxProportion = 2.5f * (float) Math.Pow(workingPower, 2);
            var proportion = Math.Min(initH2O * 0.5f, maxProportion);
            var temperatureEfficiency = Math.Min(mixture.Temperature / 1123.15f * 0.75f, 0.75f);

            var h2oRemoved = proportion * 2f;
            var oxyProduced = proportion * temperatureEfficiency;
            var hydrogenProduced = proportion * 2f * temperatureEfficiency;

            mixture.AdjustMoles(Gas.WaterVapor, -h2oRemoved);
            mixture.AdjustMoles(Gas.Oxygen, oxyProduced);
            mixture.AdjustMoles(Gas.Hydrogen, hydrogenProduced);

            var heatCap = _atmosphereSystem.GetHeatCapacity(mixture, true);
            powerLoad = Math.Max(activeLoad * (hydrogenProduced / (maxProportion * 2)), powerLoad);
        }

        if (initHyperNob > 0.01f && temperature < 150f)
        {
            var maxProportion = 1.5f * (float) Math.Pow(workingPower, 2);
            var proportion = Math.Min(initHyperNob, maxProportion);
            mixture.AdjustMoles(Gas.HyperNoblium, -proportion);
            mixture.AdjustMoles(Gas.AntiNoblium, proportion * 0.5f);

            var heatCap = _atmosphereSystem.GetHeatCapacity(mixture, true);
            powerLoad = Math.Max(activeLoad * (proportion / maxProportion), powerLoad);
        }

        if (initBZ > 0.01f)
        {
            var proportion = Math.Min(initBZ * (1f - (float) Math.Pow(Math.E, -0.5f * temperature * workingPower / Atmospherics.FireMinimumTemperatureToExist)), initBZ);
            mixture.AdjustMoles(Gas.BZ, -proportion);
            mixture.AdjustMoles(Gas.Oxygen, proportion * 0.2f);
            mixture.AdjustMoles(Gas.Halon, proportion * 2f);
            var energyReleased = proportion * Atmospherics.HalonProductionEnergy;

            var heatCap = _atmosphereSystem.GetHeatCapacity(mixture, true);
            if (heatCap > Atmospherics.MinimumHeatCapacity)
                mixture.Temperature = Math.Max((mixture.Temperature * heatCap + energyReleased) / heatCap, Atmospherics.TCMB);
            powerLoad = Math.Max(activeLoad * Math.Min(proportion / 30f, 1), powerLoad);
        }

        receiver.Load = powerLoad;

        _gasOverlaySystem.UpdateSessions();
    }
}