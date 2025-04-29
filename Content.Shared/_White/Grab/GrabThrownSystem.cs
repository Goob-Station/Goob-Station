// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage.Systems;
using Content.Shared.Damage;
using Content.Shared.Effects;
using Content.Shared.Throwing;
using Robust.Shared.Network;
using Robust.Shared.Physics.Events;
using Robust.Shared.Player;
using System.Numerics;
using Content.Goobstation.Common.Standing;
using Content.Shared._White.Standing;
using Content.Shared.Standing;
using Robust.Shared.Physics.Components;

namespace Content.Shared._White.Grab;

public sealed class GrabThrownSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedColorFlashEffectSystem _color = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly INetManager _netMan = default!;
    [Dependency] private readonly SharedLayingDownSystem _layingDown = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GrabThrownComponent, StartCollideEvent>(HandleCollide);
        SubscribeLocalEvent<GrabThrownComponent, StopThrowEvent>(OnStopThrow);
    }

    private void HandleCollide(Entity<GrabThrownComponent> ent, ref StartCollideEvent args)
    {
        if (_netMan.IsClient) // To avoid effect spam
            return;

        if (!HasComp<ThrownItemComponent>(ent))
        {
            RemComp<GrabThrownComponent>(ent);
            return;
        }

        if (ent.Comp.IgnoreEntity.Contains(args.OtherEntity))
            return;

        if (!HasComp<DamageableComponent>(ent))
            RemComp<GrabThrownComponent>(ent);

        if(!TryComp<PhysicsComponent>(ent, out var physicsComponent))
            return;

        ent.Comp.IgnoreEntity.Add(args.OtherEntity);

        var velocitySquared = args.OurBody.LinearVelocity.LengthSquared();
        var mass = physicsComponent.Mass;
        var kineticEnergy = 0.5f * mass * velocitySquared;
        var kineticEnergyDamage = new DamageSpecifier();
        kineticEnergyDamage.DamageDict.Add("Blunt", 1);
        var modNumber = Math.Floor(kineticEnergy / 100);
        kineticEnergyDamage *= Math.Floor(modNumber / 3);
        _damageable.TryChangeDamage(args.OtherEntity, kineticEnergyDamage);
        _stamina.TakeStaminaDamage(ent, (float) Math.Floor(modNumber / 2));

        _layingDown.TryLieDown(args.OtherEntity, behavior: DropHeldItemsBehavior.AlwaysDrop);

        _color.RaiseEffect(Color.Red, new List<EntityUid>() { ent }, Filter.Pvs(ent, entityManager: EntityManager));
    }

    private void OnStopThrow(EntityUid uid, GrabThrownComponent comp, StopThrowEvent args)
    {
        if (comp.DamageOnCollide != null)
            _damageable.TryChangeDamage(uid, comp.DamageOnCollide);

        if (HasComp<GrabThrownComponent>(uid))
            RemComp<GrabThrownComponent>(uid);
    }

    /// <summary>
    /// Throwing entity to the direction and ensures GrabThrownComponent with params
    /// </summary>
    /// <param name="uid">Entity to throw</param>
    /// <param name="thrower">Entity that throws</param>
    /// <param name="vector">Direction</param>
    /// <param name="grabThrownSpeed">How fast you fly when thrown</param>
    /// <param name="staminaDamage">Stamina damage on collide</param>
    /// <param name="damageToUid">Damage to entity on collide</param>
    public void Throw(
        EntityUid uid,
        EntityUid thrower,
        Vector2 vector,
        float grabThrownSpeed,
        DamageSpecifier? damageToUid = null)
    {
        var comp = EnsureComp<GrabThrownComponent>(uid);
        comp.IgnoreEntity.Add(thrower);
        comp.DamageOnCollide = damageToUid;

        _layingDown.TryLieDown(uid, behavior: DropHeldItemsBehavior.AlwaysDrop);
        _throwing.TryThrow(uid, vector, grabThrownSpeed, animated: false);
    }
}