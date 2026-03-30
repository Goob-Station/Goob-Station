// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Flammability;
using Content.Goobstation.Shared.Clothing.Components;
using Content.Server.Heretic.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Clothing.Components;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Jittering;
using Content.Shared.StatusEffectNew;
using Content.Shared.Temperature;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Clothing;

public sealed class MadnessMaskSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedStaminaSystem _stamina = default!;
    [Dependency] private readonly SharedJitteringSystem _jitter = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffect = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly HereticSystem _heretic = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MadnessMaskComponent, BeingUnequippedAttemptEvent>(OnUnequip);
        SubscribeLocalEvent<MadnessMaskComponent, InventoryRelayedEvent<GetFireProtectionEvent>>(OnGetProtection);
        SubscribeLocalEvent<MadnessMaskComponent, InventoryRelayedEvent<ModifyChangedTemperatureEvent>>(
            OnTemperatureChangeAttempt);
    }

    private void OnUnequip(Entity<MadnessMaskComponent> ent, ref BeingUnequippedAttemptEvent args)
    {
        if (_heretic.IsHereticOrGhoul(args.Unequipee))
            return;

        if (TryComp<ClothingComponent>(ent, out var clothing) && (clothing.Slots & args.SlotFlags) == SlotFlags.NONE)
            return;

        args.Cancel();
    }

    private void OnTemperatureChangeAttempt(Entity<MadnessMaskComponent> ent,
        ref InventoryRelayedEvent<ModifyChangedTemperatureEvent> args)
    {
        if (!_heretic.IsHereticOrGhoul(args.Args.Target))
            return;

        if (args.Args.TemperatureDelta > 0)
            args.Args.TemperatureDelta = 0;
    }

    private void OnGetProtection(Entity<MadnessMaskComponent> ent, ref InventoryRelayedEvent<GetFireProtectionEvent> args)
    {
        if (!_heretic.IsHereticOrGhoul(args.Args.Target) || HasComp<VeryFlammableComponent>(args.Args.Target))
            return;

        args.Args.Multiplier = -10f; // Basically ignore fire AP
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MadnessMaskComponent, ClothingComponent>();
        while (query.MoveNext(out var uid, out var mask, out var clothing))
        {
            if (clothing.InSlot == null)
                continue;

            mask.UpdateAccumulator += frameTime;

            if (mask.UpdateAccumulator < mask.UpdateTimer)
                continue;

            mask.UpdateAccumulator = 0;

            var lookup = _lookup.GetEntitiesInRange(uid, 5f);
            foreach (var look in lookup)
            {
                // heathens exclusive
                if (_heretic.IsHereticOrGhoul(look))
                    continue;

                if (HasComp<StaminaComponent>(look) && _random.Prob(.4f))
                    _stamina.TakeOvertimeStaminaDamage(look, 10f);

                if (_random.Prob(.4f))
                    _jitter.DoJitter(look, TimeSpan.FromSeconds(.5f), true, amplitude: 5, frequency: 10);

                if (_random.Prob(.25f))
                {
                    _statusEffect.TryAddStatusEffectDuration(look,
                        "StatusEffectSeeingRainbow",
                        out _,
                        TimeSpan.FromSeconds(10f));
                }
            }
        }
    }
}
