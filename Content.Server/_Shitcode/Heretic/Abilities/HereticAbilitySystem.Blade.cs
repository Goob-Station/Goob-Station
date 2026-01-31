// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 yglop <95057024+yglop@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Weapons.DelayedKnockdown;
using Content.Server.Heretic.Components.PathSpecific;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared.Damage.Components;
using Content.Shared.Heretic;
using Content.Shared.CombatMode.Pacification;
using Robust.Shared.Timing;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components; // Shitmed Change

namespace Content.Server.Heretic.Abilities;

public sealed partial class HereticAbilitySystem
{
    protected override void SubscribeBlade()
    {
        base.SubscribeBlade();

        SubscribeLocalEvent<EventHereticRealignment>(OnRealignment);
        SubscribeLocalEvent<HereticChampionStanceEvent>(OnChampionStance);
        SubscribeLocalEvent<EventHereticFuriousSteel>(OnFuriousSteel);
    }

    private void OnRealignment(EventHereticRealignment args)
    {
        if (!TryUseAbility(args))
            return;

        var ent = args.Performer;

        _statusEffect.TryRemoveStatusEffect(ent, "Stun");
        _statusEffect.TryRemoveStatusEffect(ent, "KnockedDown");
        _statusEffect.TryRemoveStatusEffect(ent, "ForcedSleep");
        _statusEffect.TryRemoveStatusEffect(ent, "Drowsiness");

        if (TryComp<StaminaComponent>(ent, out var stam))
        {
            if (stam.StaminaDamage >= stam.CritThreshold)
                _stam.ExitStamCrit(ent, stam);

            _stam.ToggleStaminaDrain(ent, args.StaminaRegenRate, true, true, args.StaminaRegenKey, ent);
            Dirty(ent, stam);
        }

        _standing.Stand(ent);
        RemCompDeferred<DelayedKnockdownComponent>(ent);
        _pulling.StopAllPulls(ent, stopPuller: false);
        if (_statusEffect.TryAddStatusEffect<PacifiedComponent>(ent, "Pacified", TimeSpan.FromSeconds(10f), true))
            _statusEffect.TryAddStatusEffect<RealignmentComponent>(ent, "Realignment", TimeSpan.FromSeconds(10f), true);

        args.Handled = true;
    }

    private void OnChampionStance(HereticChampionStanceEvent args)
    {
        foreach (var part in _body.GetBodyChildren(args.Heretic))
        {
            if (!TryComp(part.Id, out WoundableComponent? woundable))
                continue;

            woundable.CanRemove = args.Negative;
            Dirty(part.Id, woundable);
        }
    }

    private void OnFuriousSteel(EventHereticFuriousSteel args)
    {
        if (!TryUseAbility(args))
            return;

        var ent = args.Performer;

        _pblade.AddProtectiveBlade(ent);
        for (var i = 1; i < 3; i++)
        {
            Timer.Spawn(TimeSpan.FromSeconds(0.5f * i),
                () =>
                {
                    if (TerminatingOrDeleted(ent))
                        return;

                    _pblade.AddProtectiveBlade(ent);
                });
        }

        args.Handled = true;
    }
}
