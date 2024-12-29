using Robust.Shared.Random;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Server.Power.Components;
using Content.Shared.Mobs.Systems;
using Content.Server.Temperature.Components;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Popups;
using Content.Shared.Popups;
using Content.Shared._EinsteinEngines.Silicon.Systems;
using Content.Shared.Movement.Systems;
using Content.Server.Body.Components;
using Content.Shared.Mind.Components;
using System.Diagnostics.CodeAnalysis;
using Content.Server.PowerCell;
using Robust.Shared.Timing;
using Robust.Shared.Configuration;
using Robust.Shared.Utility;
using Content.Shared.PowerCell.Components;
using Content.Shared.Alert;
using Content.Shared._Goobstation.CCVar;

namespace Content.Server._EinsteinEngines.Silicon.Charge;

public sealed class SiliconChargeSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _moveMod = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly PowerCellSystem _powerCell = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SiliconComponent, ComponentStartup>(OnSiliconStartup);
    }

    public bool TryGetSiliconBattery(EntityUid silicon, [NotNullWhen(true)] out BatteryComponent? batteryComp)
    {
        batteryComp = null;
        if (!HasComp<SiliconComponent>(silicon))
            return false;


        // try get a battery directly on the inserted entity
        if (TryComp(silicon, out batteryComp)
            || _powerCell.TryGetBatteryFromSlot(silicon, out batteryComp))
            return true;


        //DebugTools.Assert("SiliconComponent does not contain Battery");
        return false;
    }

    private void OnSiliconStartup(EntityUid uid, SiliconComponent component, ComponentStartup args)
    {
        if (!HasComp<PowerCellSlotComponent>(uid))
            return;

        if (component.EntityType.GetType() != typeof(SiliconType))
            DebugTools.Assert("SiliconComponent.EntityType is not a SiliconType enum.");
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // For each siliconComp entity with a battery component, drain their charge.
        var query = EntityQueryEnumerator<SiliconComponent>();
        while (query.MoveNext(out var silicon, out var siliconComp))
        {
            if (_mobState.IsDead(silicon)
                || !siliconComp.BatteryPowered)
                continue;

            // Check if the Silicon is an NPC, and if so, follow the delay as specified in the CVAR.
            if (siliconComp.EntityType.Equals(SiliconType.Npc))
            {
                var updateTime = _config.GetCVar(GoobCVars.SiliconNpcUpdateTime);
                if (_timing.CurTime - siliconComp.LastDrainTime < TimeSpan.FromSeconds(updateTime))
                    continue;

                siliconComp.LastDrainTime = _timing.CurTime;
            }

            // If you can't find a battery, set the indicator and skip it.
            if (!TryGetSiliconBattery(silicon, out var batteryComp))
            {
                UpdateChargeState(silicon, 0, siliconComp);
                if (_alerts.IsShowingAlert(silicon, siliconComp.BatteryAlert))
                {
                    _alerts.ClearAlert(silicon, siliconComp.BatteryAlert);
                    _alerts.ShowAlert(silicon, siliconComp.NoBatteryAlert);
                }
                continue;
            }

            // If the silicon ghosted or is SSD while still being powered, skip it.
            if (TryComp<MindContainerComponent>(silicon, out var mindContComp)
                && !mindContComp.HasMind)
                continue;

            var drainRate = siliconComp.DrainPerSecond;

            // All multipliers will be subtracted by 1, and then added together, and then multiplied by the drain rate. This is then added to the base drain rate.
            // This is to stop exponential increases, while still allowing for less-than-one multipliers.
            var drainRateFinalAddi = 0f;

            // TODO: Devise a method of adding multis where other systems can alter the drain rate.
            // Maybe use something similar to refreshmovespeedmodifiers, where it's stored in the component.
            // Maybe it doesn't matter, and stuff should just use static drain?
            if (!siliconComp.EntityType.Equals(SiliconType.Npc)) // Don't bother checking heat if it's an NPC. It's a waste of time, and it'd be delayed due to the update time.
                drainRateFinalAddi += SiliconHeatEffects(silicon, siliconComp, frameTime) - 1; // This will need to be changed at some point if we allow external batteries, since the heat of the Silicon might not be applicable.

            // Ensures that the drain rate is at least 10% of normal,
            // and would allow at least 4 minutes of life with a max charge, to prevent cheese.
            drainRate += Math.Clamp(drainRateFinalAddi, drainRate * -0.9f, batteryComp.MaxCharge / 240);

            // Drain the battery.
            _powerCell.TryUseCharge(silicon, frameTime * drainRate);

            // Figure out the current state of the Silicon.
            var chargePercent = (short) MathF.Round(batteryComp.CurrentCharge / batteryComp.MaxCharge * 10f);

            UpdateChargeState(silicon, chargePercent, siliconComp);
        }
    }

    /// <summary>
    ///     Checks if anything needs to be updated, and updates it.
    /// </summary>
    public void UpdateChargeState(EntityUid uid, short chargePercent, SiliconComponent component)
    {
        component.ChargeState = chargePercent;

        RaiseLocalEvent(uid, new SiliconChargeStateUpdateEvent(chargePercent));

        _moveMod.RefreshMovementSpeedModifiers(uid);

        // If the battery was replaced and the no battery indicator is showing, replace the indicator
        if (_alerts.IsShowingAlert(uid, component.NoBatteryAlert) && chargePercent != 0)
        {
            _alerts.ClearAlert(uid, component.NoBatteryAlert);
            _alerts.ShowAlert(uid, component.BatteryAlert, chargePercent);
        }
    }

    private float SiliconHeatEffects(EntityUid silicon, SiliconComponent siliconComp, float frameTime)
    {
        if (!TryComp<TemperatureComponent>(silicon, out var temperComp)
            || !TryComp<ThermalRegulatorComponent>(silicon, out var thermalComp))
            return 0;

        // If the Silicon is hot, drain the battery faster, if it's cold, drain it slower, capped.
        var upperThresh = thermalComp.NormalBodyTemperature + thermalComp.ThermalRegulationTemperatureThreshold;
        var upperThreshHalf = thermalComp.NormalBodyTemperature + thermalComp.ThermalRegulationTemperatureThreshold * 0.5f;

        // Check if the silicon is in a hot environment.
        if (temperComp.CurrentTemperature > upperThreshHalf)
        {
            // Divide the current temp by the max comfortable temp capped to 4, then add that to the multiplier.
            var hotTempMulti = Math.Min(temperComp.CurrentTemperature / upperThreshHalf, 4);

            // If the silicon is hot enough, it has a chance to catch fire.

            siliconComp.OverheatAccumulator += frameTime;
            if (!(siliconComp.OverheatAccumulator >= 5))
                return hotTempMulti;

            siliconComp.OverheatAccumulator -= 5;

            if (!EntityManager.TryGetComponent<FlammableComponent>(silicon, out var flamComp)
                || flamComp is { OnFire: true }
                || !(temperComp.CurrentTemperature > temperComp.HeatDamageThreshold))
                return hotTempMulti;

            _popup.PopupEntity(Loc.GetString("silicon-overheating"), silicon, silicon, PopupType.MediumCaution);
            if (!_random.Prob(Math.Clamp(temperComp.CurrentTemperature / (upperThresh * 5), 0.001f, 0.9f)))
                return hotTempMulti;

            // Goobstation: Replaced by KillOnOverheatSystem
            //_flammable.AdjustFireStacks(silicon, Math.Clamp(siliconComp.FireStackMultiplier, -10, 10), flamComp);
            //_flammable.Ignite(silicon, silicon, flamComp);
            return hotTempMulti;
        }

        // Check if the silicon is in a cold environment.
        if (temperComp.CurrentTemperature < thermalComp.NormalBodyTemperature)
            return 0.5f + temperComp.CurrentTemperature / thermalComp.NormalBodyTemperature * 0.5f;

        return 0;
    }
}
