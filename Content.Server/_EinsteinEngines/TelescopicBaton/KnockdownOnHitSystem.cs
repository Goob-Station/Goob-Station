// SPDX-FileCopyrightText: 2024 Remuchi <72476615+Remuchi@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Common.Standing;
using Content.Shared.Standing;
using Content.Server.Stunnable;
using Content.Shared._EinsteinEngines.TelescopicBaton;
using Content.Shared.Mobs.Systems;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.StatusEffect;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Server._EinsteinEngines.TelescopicBaton;

public sealed class KnockdownOnHitSystem : EntitySystem
{
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!; // Goobstation

    public override void Initialize()
    {
        SubscribeLocalEvent<KnockdownOnHitComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnMeleeHit(Entity<KnockdownOnHitComponent> entity, ref MeleeHitEvent args)
    {
        if (!args.IsHit || !args.HitEntities.Any()) // Goob edit
            return;

        if (!entity.Comp.KnockdownOnHeavyAttack && args.Direction != null)
            return;

        var ev = new KnockdownOnHitAttemptEvent(false, entity.Comp.DropHeldItemsBehavior); // Goob edit
        RaiseLocalEvent(entity, ref ev);
        if (ev.Cancelled)
            return;

        List<EntityUid> knockedDown = new(); // Goobstation
        foreach (var target in
                 args.HitEntities.Where(e => !HasComp<BorgChassisComponent>(e) && _mobState.IsAlive(e))) // Goob edit
        {
            if (entity.Comp.Duration <= TimeSpan.Zero) // Goobstation
            {
                if (_standing.Down(target)) // Goobstation
                    knockedDown.Add(target);
                continue;
            }

            if (!TryComp(target, out StatusEffectsComponent? statusEffects))
                continue;

            if (_stun.TryKnockdown(target,
                entity.Comp.Duration,
                true, // refresh
                true, // autoStand
                ev.Behavior != DropHeldItemsBehavior.NoDrop)) // drop items
                knockedDown.Add(target);
        }

        if (knockedDown.Count > 0) // Goobstation
            RaiseLocalEvent(entity, new KnockdownOnHitSuccessEvent(knockedDown));
    }
}
