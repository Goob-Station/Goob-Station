// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Clothing.Components;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Drugs;
using Content.Shared.Heretic;
using Content.Shared.Jittering;
using Content.Shared.StatusEffect;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Clothing;

public sealed partial class MadnessMaskSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly SharedJitteringSystem _jitter = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffect = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var mask in EntityQuery<Shared.Clothing.Components.MadnessMaskComponent>())
        {
            mask.UpdateAccumulator += frameTime;
            if (mask.UpdateAccumulator < mask.UpdateTimer)
                continue;

            mask.UpdateAccumulator = 0;

            var lookup = _lookup.GetEntitiesInRange(mask.Owner, 5f);
            foreach (var look in lookup)
            {
                // heathens exclusive
                if (HasComp<HereticComponent>(look)
                || HasComp<GhoulComponent>(look))
                    continue;

                if (HasComp<StaminaComponent>(look) && _random.Prob(.4f))
                    _stamina.TakeStaminaDamage(look, 10f, visual: false, immediate: false);

                if (_random.Prob(.4f))
                    _jitter.DoJitter(look, TimeSpan.FromSeconds(.5f), true, amplitude: 5, frequency: 10);

                if (_random.Prob(.25f))
                    _statusEffect.TryAddStatusEffect<SeeingRainbowsComponent>(look, "SeeingRainbows", TimeSpan.FromSeconds(10f), false);
            }
        }
    }
}