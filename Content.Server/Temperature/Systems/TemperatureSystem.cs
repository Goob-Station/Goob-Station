// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Common.Temperature.Components;
using Content.Server._Goobstation.Wizard.Systems;
using Content.Server.Administration.Logs;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Temperature.Components;
using Content.Shared._DV.CosmicCult.Components; // DeltaV
using Content.Shared._Goobstation.Wizard.Spellblade;
using Content.Shared.Alert;
using Content.Shared.Atmos;
using Content.Shared.Damage;
using Content.Shared.Database;
using Content.Shared.Inventory;
using Content.Shared.Rejuvenate;
using Content.Shared.Temperature;
using Content.Shared.Projectiles;
using Content.Shared.Temperature.Components;
using Content.Shared.Temperature.Systems;
using Content.Goobstation.Shared.Temperature;
using Content.Goobstation.Common.Temperature; // goob edit

namespace Content.Server.Temperature.Systems;

public sealed partial class TemperatureSystem : SharedTemperatureSystem
{
    [Dependency] private readonly AtmosphereSystem _atmosphere = default!;

    private EntityQuery<TemperatureImmunityComponent> _immuneQuery; // DeltaV

    [Dependency] private readonly SpellbladeSystem _spellblade = default!; // Goobstation

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TemperatureComponent, AtmosExposedUpdateEvent>(OnAtmosExposedUpdate);
        SubscribeLocalEvent<TemperatureComponent, RejuvenateEvent>(OnRejuvenate);
        Subs.SubscribeWithRelay<TemperatureProtectionComponent, ModifyChangedTemperatureEvent>(OnTemperatureChangeAttempt, held: false);

        SubscribeLocalEvent<InternalTemperatureComponent, MapInitEvent>(OnInit);

        SubscribeLocalEvent<ChangeTemperatureOnCollideComponent, ProjectileHitEvent>(ChangeTemperatureOnCollide);

        SubscribeLocalEvent<SpecialLowTempImmunityComponent, TemperatureImmunityEvent>(OnCheckLowTemperatureImmunity); // Goob edit
        SubscribeLocalEvent<SpecialHighTempImmunityComponent, TemperatureImmunityEvent>(OnCheckHighTemperatureImmunity); // Goob edit
        SubscribeLocalEvent<TemperatureDamageComponent, GetTemperatureThresholdsEvent>(OnGetTemperatureThresholds); // goob edit
        SubscribeLocalEvent<TemperatureComponent, GetCurrentTemperatureEvent>(OnGetCurrentTemperature); // Goob edit

        _immuneQuery = GetEntityQuery<TemperatureImmunityComponent>(); // DeltaV

        InitializeDamage();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // conduct heat from the surface to the inside of entities with internal temperatures
        var query = EntityQueryEnumerator<InternalTemperatureComponent, TemperatureComponent>();
        while (query.MoveNext(out var uid, out var comp, out var temp))
        {
            // don't do anything if they equalised
            var diff = Math.Abs(temp.CurrentTemperature - comp.Temperature);
            if (diff < 0.1f)
                continue;

            // heat flow in W/m^2 as per fourier's law in 1D.
            var q = comp.Conductivity * diff / comp.Thickness;

            // convert to J then K
            var joules = q * comp.Area * frameTime;
            var degrees = joules / GetHeatCapacity(uid, temp);
            if (temp.CurrentTemperature < comp.Temperature)
                degrees *= -1;

            // exchange heat between inside and surface
            comp.Temperature += degrees;
            ForceChangeTemperature(uid, temp.CurrentTemperature - degrees, temp);
        }

        UpdateDamage();
    }

    public void ForceChangeTemperature(EntityUid uid, float temp, TemperatureComponent? temperature = null)
    {
        if (!TemperatureQuery.Resolve(uid, ref temperature))
            return;

        var lastTemp = temperature.CurrentTemperature;
        var delta = temperature.CurrentTemperature - temp;
        temperature.CurrentTemperature = temp;
        RaiseLocalEvent(uid, new OnTemperatureChangeEvent(temperature.CurrentTemperature, lastTemp, delta), broadcast: true);

        // Goob start

        var preEv = new BeforeTemperatureChange(
            temperature.CurrentTemperature,
            lastTemp,
            temperature.CurrentTemperature - lastTemp);
        RaiseLocalEvent(uid, ref preEv);

        var tempEv = new TemperatureImmunityEvent(temperature.CurrentTemperature);
        RaiseLocalEvent(uid, tempEv);
        temperature.CurrentTemperature = tempEv.CurrentTemperature;

        var attemptEv = new TemperatureChangeAttemptEvent(temp, lastTemp, delta);
        RaiseLocalEvent(uid, attemptEv);
        if (attemptEv.Cancelled)
            return;
        // Goob end

        RaiseLocalEvent(uid, new OnTemperatureChangeEvent(temperature.CurrentTemperature, lastTemp, delta),
            true);
    }

    public override void ChangeHeat(EntityUid uid, float heatAmount, bool ignoreHeatResistance = false, TemperatureComponent? temperature = null)
    {
        if (!TemperatureQuery.Resolve(uid, ref temperature, false))
            return;

        if (!ignoreHeatResistance)
        {
            var ev = new ModifyChangedTemperatureEvent(heatAmount, uid); // Goobstation
            RaiseLocalEvent(uid, ev);
            heatAmount = ev.TemperatureDelta;
        }


        // Goobstation start
        float lastTemp = temperature.CurrentTemperature;
        float newTemp = temperature.CurrentTemperature + heatAmount / GetHeatCapacity(uid, temperature);

        var preEv = new BeforeTemperatureChange(
            newTemp,
            lastTemp,
            newTemp - lastTemp);
        RaiseLocalEvent(uid, ref preEv);

        var tempEv = new TemperatureImmunityEvent(newTemp);
        RaiseLocalEvent(uid, tempEv);
        newTemp = tempEv.CurrentTemperature;

        float delta = newTemp - lastTemp;

        var attemptEv = new TemperatureChangeAttemptEvent(newTemp, lastTemp, delta);
        RaiseLocalEvent(uid, attemptEv);
        if (attemptEv.Cancelled)
            return;

        temperature.CurrentTemperature = newTemp;
        // Goobstation end

        RaiseLocalEvent(uid, new OnTemperatureChangeEvent(temperature.CurrentTemperature, lastTemp, delta), broadcast: true);
    }

    private void OnAtmosExposedUpdate(EntityUid uid, TemperatureComponent temperature, ref AtmosExposedUpdateEvent args)
    {
        var transform = args.Transform;

        if (transform.MapUid == null)
            return;

        var temperatureDelta = args.GasMixture.Temperature - temperature.CurrentTemperature;
        var airHeatCapacity = _atmosphere.GetHeatCapacity(args.GasMixture, false);
        var heatCapacity = GetHeatCapacity(uid, temperature);
        // TODO ATMOS: This heat transfer formula is really really wrong, it needs to be pulled out. Pending on HeatContainers.
        var heat = temperatureDelta * (airHeatCapacity * heatCapacity /
                                       (airHeatCapacity + heatCapacity));
        ChangeHeat(uid, heat * temperature.AtmosTemperatureTransferEfficiency, temperature: temperature);
    }

    private void OnInit(Entity<InternalTemperatureComponent> entity, ref MapInitEvent args)
    {
        if (!TemperatureQuery.TryComp(entity, out var temp))
            return;

        entity.Comp.Temperature = temp.CurrentTemperature;
    }

    private void OnRejuvenate(EntityUid uid, TemperatureComponent comp, RejuvenateEvent args)
    {
        ForceChangeTemperature(uid, Atmospherics.T20C, comp);
    }

    private void OnTemperatureChangeAttempt(EntityUid uid, TemperatureProtectionComponent component, ModifyChangedTemperatureEvent args)
    {
        var coefficient = args.TemperatureDelta < 0
            ? component.CoolingCoefficient
            : component.HeatingCoefficient;

        var ev = new GetTemperatureProtectionEvent(coefficient);
        RaiseLocalEvent(uid, ref ev);

        args.TemperatureDelta *= ev.Coefficient;
    }

    private void ChangeTemperatureOnCollide(Entity<ChangeTemperatureOnCollideComponent> ent, ref ProjectileHitEvent args)
    {
        ChangeHeat(args.Target, ent.Comp.Heat, ent.Comp.IgnoreHeatResistance);// adjust the temperature
    }
}
