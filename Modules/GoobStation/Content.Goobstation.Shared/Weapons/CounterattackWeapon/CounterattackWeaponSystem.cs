// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared._Shitmed.ItemSwitch;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Timing;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Weapons.CounterattackWeapon;

public sealed class CounterattackWeaponSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly UseDelaySystem _delay = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedMeleeWeaponSystem _melee = default!;
    [Dependency] private readonly SharedItemSwitchSystem _switch = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CounterattackWeaponUserComponent, AttackedEvent>(OnAttacked);

        SubscribeLocalEvent<CounterattackWeaponComponent, ComponentInit>(OnStartup);
        SubscribeLocalEvent<CounterattackWeaponComponent, EntGotInsertedIntoContainerMessage>(OnInserted);
        SubscribeLocalEvent<CounterattackWeaponComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<CounterattackWeaponComponent, EntGotRemovedFromContainerMessage>(OnRemoved);
    }

    private void OnAttacked(Entity<CounterattackWeaponUserComponent> ent, ref AttackedEvent args)
    {
        if (!_timing.IsFirstTimePredicted || ent.Owner == args.User)
            return;

        var meleeWeapon = ent.Comp.Weapons.FirstOrDefault(EntityUid.Invalid);
        if (!meleeWeapon.IsValid()
            || !TryComp<MeleeWeaponComponent>(meleeWeapon, out var meleeComp)
            || !TryComp<CounterattackWeaponComponent>(meleeWeapon, out var counterattack))
            return;

        meleeComp.NextAttack = TimeSpan.Zero;
        _melee.AttemptLightAttack(ent, meleeWeapon, meleeComp, args.User);
        meleeComp.NextAttack = TimeSpan.Zero;
        _switch.Switch(meleeWeapon, counterattack.SetItemSwitch);
        if (TryComp<UseDelayComponent>(meleeWeapon, out var delay))
            _delay.ResetAllDelays((meleeWeapon, delay));
    }

    private void OnStartup(Entity<CounterattackWeaponComponent> ent, ref ComponentInit args)
    {
        if (!_container.TryGetContainingContainer((ent, null, null), out var container)
            || !_hands.EnumerateHeld(container.Owner).Contains(ent))
            return;

        EnsureComp<CounterattackWeaponUserComponent>(container.Owner).Weapons.Add(ent);
    }

    private void OnInserted(Entity<CounterattackWeaponComponent> ent, ref EntGotInsertedIntoContainerMessage args)
    {
        if (!_hands.EnumerateHeld(args.Container.Owner).Contains(ent))
            return;

        EnsureComp<CounterattackWeaponUserComponent>(args.Container.Owner).Weapons.Add(ent);
    }

    private void OnShutdown(Entity<CounterattackWeaponComponent> ent, ref ComponentShutdown args)
    {
        if (!_container.TryGetContainingContainer((ent, null, null), out var container)
            || !TryComp<CounterattackWeaponUserComponent>(container.Owner, out var user))
            return;

        user.Weapons.Remove(ent);
        if (user.Weapons.Count == 0)
            RemComp(container.Owner, user);
    }

    private void OnRemoved(Entity<CounterattackWeaponComponent> ent, ref EntGotRemovedFromContainerMessage args)
    {
        if (!TryComp<CounterattackWeaponUserComponent>(args.Container.Owner, out var user))
            return;

        user.Weapons.Remove(ent);
        if (user.Weapons.Count == 0)
            RemComp(args.Container.Owner, user);
    }
}
