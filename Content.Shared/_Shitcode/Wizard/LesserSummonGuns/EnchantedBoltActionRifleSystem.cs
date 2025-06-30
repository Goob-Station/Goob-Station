// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Wizard.FadingTimedDespawn;
using Content.Shared.Examine;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Throwing;
using Content.Shared.Timing;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Wieldable;
using Content.Shared.Wieldable.Components;
using Robust.Shared.Network;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared._Goobstation.Wizard.LesserSummonGuns;

public sealed class EnchantedBoltActionRifleSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly ThrowingSystem _throwingSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly UseDelaySystem _useDelay = default!;
    [Dependency] private readonly SharedWieldableSystem _wieldable = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EnchantedBoltActionRifleComponent, GunShotEvent>(OnGunShot);
        SubscribeLocalEvent<EnchantedBoltActionRifleComponent, ExaminedEvent>(OnExamine);
    }

    private void OnExamine(Entity<EnchantedBoltActionRifleComponent> ent, ref ExaminedEvent args)
    {
        if (ent.Comp.Caster != null && ent.Comp.Caster != args.Examiner)
            return;

        args.PushMarkup(Loc.GetString("enchanted-rifle-guns-left", ("guns", ent.Comp.Shots)));
    }

    private void OnGunShot(Entity<EnchantedBoltActionRifleComponent> ent, ref GunShotEvent args)
    {
        var (uid, comp) = ent;

        if (_timing.IsFirstTimePredicted)
            comp.Shots--;

        var user = args.User;

        if (!TryComp(user, out HandsComponent? hands))
            return;

        var oldHand = hands.ActiveHand;

        if (oldHand == null || oldHand.HeldEntity != uid)
            return;

        if (TryComp(uid, out WieldableComponent? wieldable))
            _wieldable.TryUnwield(uid, wieldable, user, true);

        if (!_hands.TryDrop(user, oldHand, null, false, false, hands))
            return;

        // This is required so that muzzle flash faces where it should face
        _transform.SetWorldRotationNoLerp(uid, _transform.GetWorldRotation(uid) - MathHelper.PiOver4 * 3f);

        if (_net.IsClient)
            return;

        var dir = _random.NextAngle().ToVec();
        var speed = _random.NextFloat(comp.ThrowingSpeed.X, comp.ThrowingSpeed.Y);

        _throwingSystem.TryThrow(uid, dir, speed, user, 0, recoil: false);

        EnsureComp<FadingTimedDespawnComponent>(uid);

        if (comp.Shots <= 0)
            return;

        if (comp.Caster != null && comp.Caster != user)
            return;

        Hand? otherHand = null;

        foreach (var hand in _hands.EnumerateHands(user, hands))
        {
            if (hand == oldHand)
                continue;

            otherHand = hand;

            if (hand.HeldEntity == null)
                break;
        }

        var gun = Spawn(comp.Proto, _transform.GetMapCoordinates(user));

        var pickUpHand = oldHand;

        if (otherHand != null)
        {
            _hands.SetActiveHand(user, otherHand, hands);
            if (otherHand.HeldEntity != null)
                ResetDelays(gun);
            else
                pickUpHand = otherHand;
        }
        else
            ResetDelays(gun);

        if (!_hands.TryPickup(user, gun, pickUpHand, false, false, hands))
            QueueDel(gun);

        var newComp = EnsureComp<EnchantedBoltActionRifleComponent>(gun);
        newComp.Shots = comp.Shots;
        newComp.Caster = comp.Caster;
        Dirty(gun, newComp);

        if (TryComp(gun, out WieldableComponent? newWieldable))
            _wieldable.TryWield(gun, newWieldable, user, false);
    }

    private void ResetDelays(EntityUid uid)
    {
        if (TryComp(uid, out UseDelayComponent? delay))
            _useDelay.ResetAllDelays((uid, delay));
    }
}