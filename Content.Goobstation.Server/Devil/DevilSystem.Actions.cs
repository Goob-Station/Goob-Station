// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.Devil.Contract;
using Content.Goobstation.Server.Devil.Contract.Revival;
using Content.Goobstation.Server.Devil.Grip;
using Content.Goobstation.Shared.Devil;
using Content.Goobstation.Shared.Devil.Actions;
using Content.Goobstation.Shared.Devil.Condemned;
using Content.Goobstation.Shared.Devil.Contract;
using Content.Shared.Cuffs.Components;
using Content.Shared.IdentityManagement;

namespace Content.Goobstation.Server.Devil;

public sealed partial class DevilSystem
{
    private void SubscribeAbilities()
    {
        SubscribeLocalEvent<DevilComponent, CreateContractEvent>(OnContractCreated);
        SubscribeLocalEvent<DevilComponent, CreateRevivalContractEvent>(OnRevivalContractCreated);
        SubscribeLocalEvent<DevilComponent, ShadowJauntEvent>(OnShadowJaunt);
        SubscribeLocalEvent<DevilComponent, DevilGripEvent>(OnDevilGrip);
        SubscribeLocalEvent<DevilComponent, DevilPossessionEvent>(OnPossess);
    }

    private void OnContractCreated(Entity<DevilComponent> devil, ref CreateContractEvent args)
    {
        if (!TryUseAbility(args))
            return;

        var contract = SpawnAndPickup(devil, devil.Comp.ContractPrototype);
        _hands.TryPickupAnyHand(devil, contract);
        if (TryComp<DevilContractComponent>(contract, out var contractComponent))
            contractComponent.ContractOwner = args.Performer;

        PlayFwooshSound(devil);
        DoContractFlavor(devil, Identity.Name(devil, EntityManager));
    }

    private void OnRevivalContractCreated(Entity<DevilComponent> devil, ref CreateRevivalContractEvent args)
    {
        if (!TryUseAbility(args))
            return;

        var contract = SpawnAndPickup(devil, devil.Comp.RevivalContractPrototype);
        _hands.TryPickupAnyHand(devil, contract);
        if (TryComp<RevivalContractComponent>(contract, out var contractComponent))
            contractComponent.ContractOwner = args.Performer;

        PlayFwooshSound(devil);
        DoContractFlavor(devil, Identity.Name(devil, EntityManager));
    }

    private EntityUid SpawnAndPickup(Entity<DevilComponent> devil, string prototype)
    {
        var coords = Transform(devil).Coordinates;
        var entity = Spawn(prototype, coords);

        _hands.TryPickupAnyHand(devil, entity);
        return entity;
    }

    private void OnShadowJaunt(Entity<DevilComponent> devil, ref ShadowJauntEvent args)
    {
        if (!TryUseAbility(args))
            return;

        var coords = Transform(devil).Coordinates;
        Spawn(devil.Comp.JauntAnimationProto, coords);
        Spawn(devil.Comp.PentagramEffectProto, coords);

        if (TryComp<CuffableComponent>(devil, out var cuffableComponent))
            _container.EmptyContainer(cuffableComponent.Container, true);

        _poly.PolymorphEntity(devil, devil.Comp.JauntEntityProto);
    }

    private void OnDevilGrip(Entity<DevilComponent> devil, ref DevilGripEvent args)
    {
        if (!TryUseAbility(args))
            return;

        if (devil.Comp.DevilGrip != null)
        {
            foreach (var item in _hands.EnumerateHeld(devil.Owner))
            {
                if (!HasComp<DevilGripComponent>(item))
                    continue;

                QueueDel(item);
                return;
            }
        }

        var grasp = Spawn(devil.Comp.GripPrototype, Transform(devil).Coordinates);
        if (!_hands.TryPickupAnyHand(devil, grasp))
            QueueDel(grasp);

        devil.Comp.DevilGrip = args.Action.Owner;
    }

    private void OnPossess(Entity<DevilComponent> devil, ref DevilPossessionEvent args)
    {
        if (!TryComp<CondemnedComponent>(args.Target, out var condemned) || condemned.SoulOwnedNotDevil)
        {
            var message = Loc.GetString("invalid-possession-target");
            _popup.PopupEntity(message, devil, devil);
            return;
        }

        if (!TryUseAbility(args))
            return;

        if (devil.Comp.PowerLevel != DevilPowerLevel.None)
            devil.Comp.PossessionDuration *= (int) devil.Comp.PowerLevel;

        // Only mark as handled if possession succeeds to avoid wasting the cooldown.
        if (!_possession.TryPossessTarget(args.Target, args.Performer, devil.Comp.PossessionDuration, true, polymorphPossessor: true))
            return;

        var targetCoords = Transform(args.Target).Coordinates;
        Spawn(devil.Comp.JauntAnimationProto, targetCoords);
        Spawn(devil.Comp.PentagramEffectProto, targetCoords);

        args.Handled = true;
    }
}
