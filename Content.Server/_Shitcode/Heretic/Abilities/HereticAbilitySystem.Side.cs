// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Body.Components;
using Content.Shared.Heretic;
using Content.Shared.Mobs.Components;

namespace Content.Server.Heretic.Abilities;

public sealed partial class HereticAbilitySystem
{
    protected override void SubscribeSide()
    {
        base.SubscribeSide();

        SubscribeLocalEvent<EventHereticCleave>(OnCleave);
    }

    private void OnCleave(EventHereticCleave args)
    {
        if (!TryUseAbility(args))
            return;

        args.Handled = true;

        if (!args.Target.IsValid(EntityManager))
            return;

        Spawn(args.Effect, args.Target);

        var bloodQuery = GetEntityQuery<BloodstreamComponent>();

        var hasTargets = false;

        var targets = Lookup.GetEntitiesInRange<MobStateComponent>(args.Target, args.Range, LookupFlags.Dynamic);
        foreach (var (target, _) in targets)
        {
            if (target == args.Performer)
                continue;

            hasTargets = true;

            _dmg.TryChangeDamage(target, args.Damage, true, origin: args.Performer);

            if (!bloodQuery.TryComp(target, out var blood))
                continue;

            _blood.TryModifyBloodLevel((target, blood), args.BloodModifyAmount);
            _blood.TryModifyBleedAmount((target, blood), blood.MaxBleedAmount);
        }

        if (hasTargets)
            _aud.PlayPvs(args.Sound, args.Target);
    }
}
