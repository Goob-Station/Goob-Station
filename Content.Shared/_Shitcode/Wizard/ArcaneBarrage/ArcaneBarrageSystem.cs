// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Hands;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory.Events;
using Content.Shared.Timing;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Shared._Goobstation.Wizard.ArcaneBarrage;

public sealed class ArcaneBarrageSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly UseDelaySystem _useDelay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ArcaneBarrageComponent, GunShotEvent>(OnBarrageShot);
        SubscribeLocalEvent<ArcaneBarrageComponent, OnEmptyGunShotEvent>(OnBarrageEmptyShot);
        SubscribeLocalEvent<ArcaneBarrageComponent, ContainerGettingRemovedAttemptEvent>(OnRemoveAttempt);
        SubscribeLocalEvent<ArcaneBarrageComponent, GotUnequippedEvent>(OnUnequip);
        SubscribeLocalEvent<ArcaneBarrageComponent, GotUnequippedHandEvent>(OnUnequipHand);
    }

    private void OnRemoveAttempt(EntityUid uid, ArcaneBarrageComponent comp, ContainerGettingRemovedAttemptEvent args)
    {
        if (!_timing.ApplyingState && comp.Unremoveable)
            args.Cancel();
    }

    private void OnUnequip(EntityUid uid, ArcaneBarrageComponent comp, GotUnequippedEvent args)
    {
        if (_net.IsServer && comp.Unremoveable)
            QueueDel(uid);
    }

    private void OnUnequipHand(EntityUid uid, ArcaneBarrageComponent comp, GotUnequippedHandEvent args)
    {
        if (_net.IsServer && comp.Unremoveable)
            QueueDel(uid);
    }

    private void OnBarrageEmptyShot(Entity<ArcaneBarrageComponent> ent, ref OnEmptyGunShotEvent args)
    {
        if (_net.IsServer)
            QueueDel(ent);
    }

    private void OnBarrageShot(Entity<ArcaneBarrageComponent> ent, ref GunShotEvent args)
    {
        if (_timing.ApplyingState || !Exists(ent))
            return;

        var user = args.User;

        if (!TryComp(user, out HandsComponent? hands))
            return;

        var oldHand = _hands.GetActiveHand((user, hands));
        string? otherHand = null;

        foreach (var hand in _hands.EnumerateHands((user, hands)))
        {
            if (hand == oldHand)
                continue;

            otherHand = hand;

            if (_hands.GetHeldItem((user, hands), hand) == null)
                break;
        }

        if (otherHand != null)
            _hands.SetActiveHand((user, hands), otherHand);
        else
        {
            ResetDelays(ent);
            return;
        }

        if (_hands.GetHeldItem((user, hands), otherHand) != null)
        {
            ResetDelays(ent);
            return;
        }

        if (oldHand == null || _hands.GetHeldItem((user, hands), oldHand) != ent)
        {
            ResetDelays(ent);
            return;
        }

        ent.Comp.Unremoveable = false;
        if (!_hands.TryDrop((user, hands), oldHand, null, false, false))
        {
            ResetDelays(ent);
            return;
        }
        if (!_hands.TryPickup(user, ent, otherHand, false) && _net.IsServer)
            QueueDel(ent);
        ent.Comp.Unremoveable = true;
    }

    private void ResetDelays(EntityUid uid)
    {
        if (TryComp(uid, out UseDelayComponent? delay))
            _useDelay.ResetAllDelays((uid, delay));
    }
}
