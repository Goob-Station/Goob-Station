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

        var oldHand = hands.ActiveHand;
        Hand? otherHand = null;

        foreach (var hand in _hands.EnumerateHands(user, hands))
        {
            if (hand == oldHand)
                continue;

            otherHand = hand;

            if (hand.HeldEntity == null)
                break;
        }

        if (otherHand != null)
            _hands.SetActiveHand(user, otherHand, hands);
        else
        {
            ResetDelays(ent);
            return;
        }

        if (otherHand.HeldEntity != null)
        {
            ResetDelays(ent);
            return;
        }

        if (oldHand == null || oldHand.HeldEntity != ent)
        {
            ResetDelays(ent);
            return;
        }

        ent.Comp.Unremoveable = false;
        if (!_hands.TryDrop(user, oldHand, null, false, false, hands))
        {
            ResetDelays(ent);
            return;
        }
        if (!_hands.TryPickup(user, ent, otherHand, false, false, hands) && _net.IsServer)
            QueueDel(ent);
        ent.Comp.Unremoveable = true;
    }

    private void ResetDelays(EntityUid uid)
    {
        if (TryComp(uid, out UseDelayComponent? delay))
            _useDelay.ResetAllDelays((uid, delay));
    }
}
