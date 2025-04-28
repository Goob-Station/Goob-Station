// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Server._Goobstation.Wizard.Components;
using Content.Server.Temperature.Components;
using Content.Server.Temperature.Systems;
using Content.Shared._Goobstation.Wizard.Traps;
using Content.Shared.Damage;
using Content.Shared.Projectiles;
using Content.Shared.Temperature;
using Content.Shared.Whitelist;
using Robust.Shared.Random;

namespace Content.Server._Goobstation.Wizard.Systems;

public sealed class IceCubeSystem : SharedIceCubeSystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    [Dependency] private readonly TemperatureSystem _temperature = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<IceCubeComponent, OnTemperatureChangeEvent>(OnTemperatureChange);
        SubscribeLocalEvent<IceCubeComponent, DamageChangedEvent>(OnDamageChanged);
        SubscribeLocalEvent<IceCubeOnProjectileHitComponent, ProjectileHitEvent>(OnHit);
    }

    private void OnHit(Entity<IceCubeOnProjectileHitComponent> ent, ref ProjectileHitEvent args)
    {
        if (_whitelist.IsValid(ent.Comp.Whitelist, args.Target))
            EnsureComp<IceCubeComponent>(args.Target);
    }

    private void OnDamageChanged(Entity<IceCubeComponent> ent, ref DamageChangedEvent args)
    {
        var (uid, comp) = ent;

        if (!TryComp(uid, out TemperatureComponent? temperature))
            return;

        if (args is not { DamageIncreased: true, DamageDelta: not null })
            return;

        if (args.DamageDelta.DamageDict.TryGetValue("Heat", out var heat))
        {
            _temperature.ForceChangeTemperature(uid,
                MathF.Min(comp.UnfreezeTemperatureThreshold + 10f,
                    temperature.CurrentTemperature + heat.Float() * comp.TemperaturePerHeatDamageIncrease),
                temperature);
        }

        var realDamage = args.DamageDelta.DamageDict.Where(kvp => kvp.Key is "Blunt" or "Slash" or "Piercing" or "Heat")
            .Sum(kvp => kvp.Value.Float());

        if (realDamage <= 0f)
            return;

        ent.Comp.SustainedDamage += realDamage;
        if (ent.Comp.SustainedDamage <= comp.DamageMeltProbabilityThreshold)
            return;

        var probability = Math.Clamp(ent.Comp.SustainedDamage * ent.Comp.SustainedDamageMeltProbabilityMultiplier /
            100f * InverseLerp(ent.Comp.FrozenTemperature,
            ent.Comp.UnfrozenTemperature,
            temperature.CurrentTemperature),
            0.2f, // At least 20%
            1f);

        if (_random.Prob(probability))
            RemCompDeferred(ent.Owner, ent.Comp);

        return;

        float InverseLerp(float min, float max, float value)
        {
            return max <= min ? 1f : Math.Clamp((value - min) / (max - min), 0f , 1f);
        }
    }

    private void OnTemperatureChange(Entity<IceCubeComponent> ent, ref OnTemperatureChangeEvent args)
    {
        if (args.TemperatureDelta > 0f && args.CurrentTemperature > ent.Comp.UnfreezeTemperatureThreshold)
            RemCompDeferred(ent.Owner, ent.Comp);
    }

    protected override void Startup(Entity<IceCubeComponent> ent)
    {
        base.Startup(ent);

        var (uid, comp) = ent;

        if (!TryComp(uid, out TemperatureComponent? temperature))
            return;

        _temperature.ForceChangeTemperature(uid,
            MathF.Min(temperature.CurrentTemperature, comp.FrozenTemperature),
            temperature);
    }

    protected override void Shutdown(Entity<IceCubeComponent> ent)
    {
        base.Shutdown(ent);

        var (uid, comp) = ent;

        if (!TryComp(uid, out TemperatureComponent? temperature))
            return;

        _temperature.ForceChangeTemperature(uid,
            MathF.Max(temperature.CurrentTemperature, comp.UnfrozenTemperature),
            temperature);
    }
}
